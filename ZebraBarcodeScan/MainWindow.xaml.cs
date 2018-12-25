using CoreScanner;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace ZebraBarcodeScan {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		readonly bool[] m_arSelectedTypes;
		long m_nResultLineCount;
		int m_nTotalScanners;
		Scanner[] m_arScanners;
		XmlReader m_xml;

		CCoreScannerClass m_pCoreScanner;
		bool m_bSuccessOpen;//Is open success

		short[] m_arScannerTypes;
		short m_nNumberOfTypes;

		//total scanners in types of SCANNER_TYPES_SNAPI, SCANNER_TYPES_SSI, SCANNER_TYPES_IBMHID, SCANNER_TYPES_NIXMODB, SCANNER_TYPES_HIDKB
		int[] m_nArTotalScannersInType;

		private readonly string hintConnectionUnseccessfull =
			"Для продолжения работы убедитесь, что сканер физически подключен и корректно настроен." + Environment.NewLine +
			"В случае невозможности самостоятельного исправления проблем с подключением," + Environment.NewLine +
			"необходимо обратиться в службу технической поддержки пользователей.";

		private readonly string hintConnectionSuccessfull = "" +
			"Для продолжения отсканируйте QR-код на купоне...";

		readonly List<string> claimlist = new List<string>();
		private readonly FirebirdClient firebirdClient = new FirebirdClient(
				Properties.Settings.Default.MisDbAddress,
				Properties.Settings.Default.MisDbName,
				Properties.Settings.Default.MisDbUser,
				Properties.Settings.Default.MisDbPassword);

		private readonly string sqlQueryCheckCode = Properties.Settings.Default.MisDbQueryCheckCode;
		private readonly string sqlQueryCheckHistnum = Properties.Settings.Default.MisDbQueryCheckHistnum;
		private readonly string sqlUpdateCode = Properties.Settings.Default.MisDbUpdateCode;

        private string currentCodeSeries = string.Empty;
        private string currentCodeID = string.Empty;


		public MainWindow() {
			InitializeComponent();

			m_nResultLineCount = 0;
			m_bSuccessOpen = false;
			m_nTotalScanners = 0;
			m_nArTotalScannersInType = new int[TOTAL_SCANNER_TYPES];
			InitScannersCount();
			m_arScanners = new Scanner[MAX_NUM_DEVICES];
			for (int i = 0; i < MAX_NUM_DEVICES; i++) {
				Scanner scanr = new Scanner();
				m_arScanners.SetValue(scanr, i);
			}

			m_xml = new XmlReader();
			m_nNumberOfTypes = 0;
			m_arScannerTypes = new short[TOTAL_SCANNER_TYPES];
			m_arSelectedTypes = new bool[TOTAL_SCANNER_TYPES];

			SetControls();

			try {
				m_pCoreScanner = new CCoreScannerClass();
                m_pCoreScanner.BarcodeEvent += new _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);
            } catch (Exception e) {
                MessageBox.Show("Не установлены драйвера Zebra_CoreScanner_Driver: " + 
                    e.Message + Environment.NewLine + e.StackTrace, "CoreScannerError", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

			Loaded += MainWindow_Loaded;
		}



		private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			bool result = PerformGetScanner();

            //temp
            //result = true;

            if (!result) {
				DispatcherTimer dispatcherTimer = new DispatcherTimer {
					Interval = TimeSpan.FromSeconds(5)
				};

				dispatcherTimer.Tick += DispatcherTimer_Tick;
				dispatcherTimer.Start();
			}

            UpdateStatusTextBlocks(result);

            //temp
            //UpdateCodeInfo("999-4-26A4C737-9FCE-419D-9038-8BDD2F0E8E1B");
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e) {
			if (PerformGetScanner()) {
				UpdateStatusTextBlocks(true);
				(sender as DispatcherTimer).Stop();
			}
		}

		private void UpdateStatusTextBlocks(bool isSuccess) {
			BorderStatus.Background = isSuccess ? Brushes.LightGreen : Brushes.Orange;
			TextBlockStatus.Text = isSuccess ? "Подключен" : "Не подключен";
			TextBlockHint.Text = isSuccess ? hintConnectionSuccessfull : hintConnectionUnseccessfull;
			TextBlockHint.Visibility = Visibility.Visible;
			GridScannedQR.Visibility = Visibility.Hidden;
		}

		private void UpdateCodeInfo(string code) {
			bool isCodeFormatError = false;
            bool isDbError = false;
            bool isCodeNotAvailable = false;

            string uuid = string.Empty;
            DateTime? date_start = null;
            DateTime? date_end = null;
            string comment = string.Empty;
            string use_status = string.Empty;

            if (!code.Contains("-"))
				isCodeFormatError = true;
			else {
				string[] codeArray = code.Split('-');

				if (codeArray.Length < 3)
					isCodeFormatError = true;
				else {
                    currentCodeSeries = codeArray[0];
                    currentCodeID = codeArray[1];

                    DataTable dataTable = firebirdClient.GetDataTable(sqlQueryCheckCode, new Dictionary<string, string> {
						{"@series", currentCodeSeries },
						{"@id", currentCodeID }
					});

					if (dataTable.Rows.Count == 0) {
                        isDbError = true;
                        MessageBox.Show(this, "По отсканированному коду не найдено данных. " +
                            "Возможно отсканирован некорректный код.", "", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
					} else if (dataTable.Rows.Count > 1) {
                        isDbError = true;
                        MessageBox.Show(this, "По отсканированному коду найдено несколько записей. " +
                            "Необходимо обратиться в службу технической поддержки.", "",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else {
                        try {
                            DataRow dataRow = dataTable.Rows[0];
                            uuid = dataRow["UUID"].ToString();

                            string scannedUUID = code.Replace(codeArray[0] + "-" + codeArray[1] + "-", "");
                            if (!scannedUUID.Equals(uuid)) {
                                use_status = "Код недействителен";
                                MessageBox.Show(this, "Секретный ключ отсканированного кода " +
                                    "не совпадает с данными в базе. Код недействителен.",
                                    "", MessageBoxButton.OK, MessageBoxImage.Error);
                                isCodeNotAvailable = true;
                            } else {

                                string date_start_string = dataRow["DATE_START"].ToString();
                                string date_end_string = dataRow["DATE_END"].ToString();

                                if (!string.IsNullOrEmpty(date_start_string))
                                    date_start = DateTime.Parse(date_start_string);

                                if (!string.IsNullOrEmpty(date_end_string))
                                    date_end = DateTime.Parse(date_end_string);

                                comment = dataRow["COMMENT"].ToString();
                                use_status = dataRow["USE_STATUS"].ToString();

                                if (use_status.Equals("1")) {
                                    isCodeNotAvailable = true;

                                    string use_date = dataRow["USE_DATE"].ToString();
                                    string use_system_name = dataRow["USE_SYSTEM_NAME"].ToString();

                                    use_status = "Код был активирован ранее";
                                    MessageBox.Show(this, "Отсканированный код был активирован ранее:" +
                                        Environment.NewLine + "Дата активации: " + use_date.Replace(" 0:00:00", "") +
                                        Environment.NewLine + "Имя системы: " + use_system_name, "",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                } else {
                                    if (date_start.HasValue && date_start.Value > DateTime.Now) {
                                        use_status = "Срок действия кода еще не начался";
                                        MessageBox.Show(this, "Срок действия отсканированного кода еще не начался",
                                            "", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        isCodeNotAvailable = true;
                                    } else if (date_end.HasValue && date_end.Value.AddDays(1) < DateTime.Now) {
                                        use_status = "Срок действия кода истек";
                                        MessageBox.Show(this, "Срок действия отсканированного кода истек",
                                            "", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        isCodeNotAvailable = true;
                                    }
                                }

                                if (!isCodeNotAvailable)
                                    use_status = "Доступен для активации";
                            }
                        } catch (Exception e) {
                            MessageBox.Show(this, e.Message + Environment.NewLine + e.StackTrace,
                                "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                            isDbError = true;
                        }
                    }
				}
			}

			if (isCodeFormatError) {
				MessageBox.Show(this, "Формат отсканированного кода не совпадает с требуемым", "", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			TextBlockHint.Visibility = Visibility.Hidden;

			TextBoxCode.Text = code;
			TextBoxStatus.Text = use_status;
			TextBoxDateBegin.Text = date_start.HasValue ? date_start.Value.ToLongDateString() : string.Empty;
			TextBoxDateEnd.Text = date_end.HasValue ? date_end.Value.ToLongDateString() : string.Empty;
			TextBoxComment.Text = comment;
			TextBoxHistnum.Text = string.Empty;

            ButtonActivate.IsEnabled = !isCodeFormatError && !isDbError && !isCodeNotAvailable;
			ButtonActivate.Visibility = Visibility.Visible;
			GridActivateCode.Visibility = Visibility.Collapsed;
			GridConfirmPatient.Visibility = Visibility.Collapsed;
			GridScannedQR.Visibility = Visibility.Visible;
		}

		private void ButtonActivate_Click(object sender, RoutedEventArgs e) {
			ButtonActivate.Visibility = Visibility.Collapsed;
			GridActivateCode.Visibility = Visibility.Visible;
		}

		private void ButtonEnterHistnum_Click(object sender, RoutedEventArgs e) {
            string histnum = TextBoxHistnum.Text;

            DataTable dataTable = firebirdClient.GetDataTable(sqlQueryCheckHistnum, new Dictionary<string, string> {
                {"@histnum", histnum }
            });

            if (dataTable.Rows.Count == 0) {
                MessageBox.Show(this, "Не найдено пациентов по введеному номеру истории болезни. Возможно номер введен неверно",
                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dataTable.Rows.Count > 1) {
                MessageBox.Show(this, "По введенному номеру истории болезни найден более чем один пациент. Обратитесь в службу технической поддержки",
                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string patientName = string.Empty;
            string patientBirthday = string.Empty;
            DataRow dataRow = dataTable.Rows[0];

            try {
                patientName = dataRow["FULLNAME"].ToString();
                patientBirthday = dataRow["BDATE"].ToString().Replace(" 0:00:00", "");
            } catch (Exception exc) {
                MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace,
                    "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

			GridActivateCode.Visibility = Visibility.Collapsed;

			TextBoxPatientName.Text = patientName;
			TextBoxPatientBirthday.Text = patientBirthday;

			GridConfirmPatient.Visibility = Visibility.Visible;
		}

		private void ButtonCloseCode_Click(object sender, RoutedEventArgs e) {
			GridScannedQR.Visibility = Visibility.Hidden;
			TextBlockHint.Visibility = Visibility.Visible;
		}

		private void ButtonActivateCode_Click(object sender, RoutedEventArgs e) {
            bool isUpdateOk = firebirdClient.ExecuteUpdateQuery(sqlUpdateCode, new Dictionary<string, object> {
                { "@date", DateTime.Now.ToString() },
                { "@histnum", TextBoxHistnum.Text },
                { "@system", Environment.MachineName },
                { "@id", currentCodeID },
                { "@series", currentCodeSeries }
            });

            if (isUpdateOk) {
                MessageBox.Show(this, "Активация кода прошла успешно!", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                ButtonCloseCode_Click(null, null);
            } else {
                MessageBox.Show(this, "Не удалось активировать код. Обратитесь в службу технической поддержки", 
                    string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
		}

		private void ButtonCancelHistnum_Click(object sender, RoutedEventArgs e) {
			GridConfirmPatient.Visibility = Visibility.Collapsed;
			GridActivateCode.Visibility = Visibility.Visible;
		}

		void OnBarcodeEvent(short eventType, ref string scanData) {
			try {
				string tmpScanData = scanData;

				UpdateResults("Barcode Event fired");
				ShowBarcodeLabel(tmpScanData);

				txtBarcode.Dispatcher.Invoke(() => {
					txtBarcode.Text = IndentXmlString(tmpScanData);
				});
			} catch (Exception e) {
				MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ShowBarcodeLabel(string strXml) {
			System.Diagnostics.Debug.WriteLine("Initial XML" + strXml);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(strXml);

			string strData = string.Empty;
			string barcode = xmlDoc.DocumentElement.GetElementsByTagName("datalabel").Item(0).InnerText;
			string symbology = xmlDoc.DocumentElement.GetElementsByTagName("datatype").Item(0).InnerText;
			string[] numbers = barcode.Split(' ');

			foreach (string number in numbers) {
				if (string.IsNullOrEmpty(number)) {
					break;
				}

				strData += ((char)Convert.ToInt32(number, 16)).ToString();
			}

			txtBarcodeLbl.Dispatcher.Invoke(() => {
				txtBarcodeLbl.Clear();
				txtBarcodeLbl.Text = strData;
			});

			Application.Current.Dispatcher.Invoke(() => {
				UpdateCodeInfo(strData);
			});

			txtSyblogy.Dispatcher.Invoke(() => {
				txtSyblogy.Text = GetSymbology(Convert.ToInt32(symbology));
			});
		}

		private void InitScannersCount() {
			for (int index = 0; index < 6; index++) {
				m_nArTotalScannersInType[index] = 0;
			}
		}

		private void ButtonGetScanners_Click(object sender, RoutedEventArgs e) {
			PerformGetScanner();
		}


		private bool PerformGetScanner() {
			FilterScannerList();
			bool resultConnect = MakeConnectCtrl();
			bool resultRegister = RegisterForEvents();
			bool resultShow = ShowScanners();

			return resultConnect && resultRegister && resultShow;
		}

		private void FilterScannerList() {
			for (int index = 0; index < TOTAL_SCANNER_TYPES; index++) {
				m_arSelectedTypes[index] = false;
			}

			m_arSelectedTypes[SCANNER_TYPES_ALL - 1] = true;
		}

		private bool MakeConnectCtrl() {
			if (STR_FIND == btnGetScanners.Content.ToString()) {
				Connect();

				if (m_bSuccessOpen) {
					SetControls();
					btnGetScanners.Content = STR_REFRESH;

					return true;
				}
			} else if (STR_REFRESH == btnGetScanners.Content.ToString()) {
				Disconnect();

				if (!m_bSuccessOpen) {
					SetControls();
					btnGetScanners.Content = STR_FIND;
					ClearAllRsmData();
				}

				Connect();

				if (m_bSuccessOpen) {
					SetControls();
					btnGetScanners.Content = STR_REFRESH;

					return true;
				}
			}

			return false;
		}


		private void Connect() {
			if (m_bSuccessOpen) {
				return;
			}
			int appHandle = 0;
			GetSelectedScannerTypes();
			int status = STATUS_FALSE;

			try {
				m_pCoreScanner.Open(appHandle, m_arScannerTypes, m_nNumberOfTypes, out status);
				DisplayResult(status, "OPEN");
				if (STATUS_SUCCESS == status) {
					m_bSuccessOpen = true;
				}
			} catch (Exception exp) {
				MessageBox.Show("Error OPEN - " + exp.Message, APP_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
			} finally {
				if (STATUS_SUCCESS == status) {
					SetControls();
				}
			}
		}
		
		private void DisplayResult(int status, string strCmd) {
			switch (status) {
				case STATUS_SUCCESS:
					UpdateResults(strCmd + " - Command success.");
					break;
				case STATUS_LOCKED:
					UpdateResults(strCmd + " - Command failed. Device is locked by another application.");
					break;
				default:
					UpdateResults(strCmd + " - Command failed. Error:" + status.ToString());
					break;
			}
		}

		private void UpdateResults(string strOut) {
			m_nResultLineCount++;

			txtResults.Dispatcher.Invoke(() => {
				txtResults.AppendText(m_nResultLineCount.ToString() + ". " + strOut + Environment.NewLine);
			});

			//if (txtResults.InvokeRequired) {
			//	txtResults.Invoke(new MethodInvoker(delegate {
			//		txtResults.AppendText(m_nResultLineCount.ToString() + ". " + strOut + Environment.NewLine);
			//	}));
			//} else {
			//txtResults.AppendText(m_nResultLineCount.ToString() + ". " + strOut + Environment.NewLine);
			//}

			toolStripStatusLbl.Dispatcher.Invoke(() => {
				toolStripStatusLbl.Text = strOut + "        ";
			});
		}

		private void GetSelectedScannerTypes() {
			m_nNumberOfTypes = 0;
			for (int index = 0, k = 0; index < TOTAL_SCANNER_TYPES; index++) {
				if (m_arSelectedTypes[index]) {
					m_nNumberOfTypes++;
					switch (index + 1) {
						case SCANNER_TYPES_ALL:
							m_arScannerTypes[k++] = SCANNER_TYPES_ALL;
							return;

						case SCANNER_TYPES_SNAPI:
							m_arScannerTypes[k++] = SCANNER_TYPES_SNAPI;
							break;

						case SCANNER_TYPES_SSI:
							m_arScannerTypes[k++] = SCANNER_TYPES_SSI;
							break;

						case SCANNER_TYPES_NIXMODB:
							m_arScannerTypes[k++] = SCANNER_TYPES_NIXMODB;
							break;

						case SCANNER_TYPES_RSM:
							m_arScannerTypes[k++] = SCANNER_TYPES_RSM;
							break;

						case SCANNER_TYPES_IMAGING:
							m_arScannerTypes[k++] = SCANNER_TYPES_IMAGING;
							break;

						case SCANNER_TYPES_IBMHID:
							m_arScannerTypes[k++] = SCANNER_TYPES_IBMHID;
							break;

						case SCANNER_TYPES_HIDKB:
							m_arScannerTypes[k++] = SCANNER_TYPES_HIDKB;
							break;

						case SCALE_TYPES_SSI_BT:
							m_arScannerTypes[k++] = SCALE_TYPES_SSI_BT;
							break;

						default:
							break;
					}
				}
			}
		}
		
		private void Disconnect() {
			if (m_bSuccessOpen) {
				int appHandle = 0;
				int status = STATUS_FALSE;
				try {
					m_pCoreScanner.Close(appHandle, out status);
					DisplayResult(status, "CLOSE");
					if (STATUS_SUCCESS == status) {
						m_bSuccessOpen = false;
						lstvScanners.Items.Clear();
						//combSlcrScnr.Items.Clear();
						m_nTotalScanners = 0;
						InitScannersCount();
						UpdateScannerCountLabels();
						SetControls();
					}
				} catch (Exception exp) {
					MessageBox.Show("CLOSE Error - " + exp.Message, APP_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void UpdateScannerCountLabels() {
			toolStripStatusLblTotal.Text = "Total = " + m_nTotalScanners.ToString();
			toolStripStatusLblSnapi.Text = "SNAPI = " + m_nArTotalScannersInType[0].ToString();
			toolStripStatusLblSsi.Text = "SSI = " + m_nArTotalScannersInType[1].ToString();
			toolStripStatusLblIbmhid.Text = "IBMHID = " + m_nArTotalScannersInType[2].ToString();
			toolStripStatusLblNxmdb.Text = "NXMODB = " + m_nArTotalScannersInType[3].ToString();
			toolStripStatusLblHidkb.Text = "HIDKB = " + m_nArTotalScannersInType[4].ToString();
			toolStripStatusIBMTT.Text = "IBMTT = " + m_nArTotalScannersInType[5].ToString();
		}

		private void SetControls() {
			bool bEnable = IsMotoConnected();
			//filterScnrs.Enabled = bEnable;
			//grpScanners.Enabled = bEnable;
			//grpAsync.Enabled = bEnable;
			//grpScannerProp.Enabled = bEnable;
			//grpTrigger.Enabled = bEnable;
			grpboxBarcodeLbl.IsEnabled = bEnable;
			txtBarcode.IsEnabled = bEnable;
			//grpImageVideo.Enabled = bEnable;
			//grpScnActions.Enabled = bEnable;
			//grpRSM.Enabled = bEnable;
			//gbAdvanced.Enabled = bEnable;
			//grpFrmWrUpdate.Enabled = bEnable;
			//grpCustomDecodeTone.Enabled = bEnable;
			//grpMiscOther.Enabled = bEnable;
			//grpScale.Enabled = bEnable;
			//grpIDC.Enabled = bEnable;
			//grpScan2Connect.Enabled = bEnable;
			//pbxImageVideo.Image = null;
		}

		private bool IsMotoConnected() {
			return m_bSuccessOpen;
		}


		private void ClearAllRsmData() {
			//clear ALL cells of all rows
			//int nRowCount = dgvAttributes.RowCount;
			//if (0 < nRowCount) {
			//	for (int i = 0; i < nRowCount; i++) {
			//		dgvAttributes.Rows[i].Cells[0].Value = "";
			//		dgvAttributes.Rows[i].Cells[1].Value = "";
			//		dgvAttributes.Rows[i].Cells[2].Value = "";
			//		dgvAttributes.Rows[i].Cells[3].Value = "";
			//		dgvAttributes.Rows[i].Selected = false;
			//	}
			//}

			foreach (Scanner scanr in m_arScanners) {
				m_xml.clear_scanner_attributes(scanr);
			}
		}

		private bool RegisterForEvents() {
			bool result = false;

			if (IsMotoConnected()) {
				string strEvtIDs = GetRegUnregIDs(out int nEvents);
				string inXml = "<inArgs>" +
									"<cmdArgs>" +
									"<arg-int>" + nEvents + "</arg-int>" +
									"<arg-int>" + strEvtIDs + "</arg-int>" +
									"</cmdArgs>" +
									"</inArgs>";

				int opCode = REGISTER_FOR_EVENTS;
				int status = STATUS_FALSE;
				result = ExecCmd(opCode, ref inXml, out string outXml, out status);
				DisplayResult(status, "REGISTER_FOR_EVENTS");
			}

			return result;
		}

		private bool ExecCmd(int opCode, ref string inXml, out string outXml, out int status) {
			outXml = "";
			status = STATUS_FALSE;
			if (m_bSuccessOpen) {
				try {
					m_pCoreScanner.ExecCommand(opCode, ref inXml, out outXml, out status);

					return true;
				} catch (Exception ex) {
					DisplayResult(status, "EXEC_COMMAND");
					UpdateResults("..." + ex.Message.ToString());
				}
			}

			return false;
		}

		private string GetRegUnregIDs(out int nEvents) {
			string strIDs = "";
			nEvents = NUM_SCANNER_EVENTS;
			strIDs = SUBSCRIBE_BARCODE.ToString();
			strIDs += "," + SUBSCRIBE_IMAGE.ToString();
			strIDs += "," + SUBSCRIBE_VIDEO.ToString();
			strIDs += "," + SUBSCRIBE_RMD.ToString();
			strIDs += "," + SUBSCRIBE_PNP.ToString();
			strIDs += "," + SUBSCRIBE_OTHER.ToString();
			return strIDs;
		}

		private bool ShowScanners() {
			int opCode = CLAIM_DEVICE;
			string inXml = string.Empty;
			string outXml = "";
			int status = STATUS_FALSE;
			lstvScanners.Items.Clear();

			m_arScanners.Initialize();
			if (m_bSuccessOpen) {
				m_nTotalScanners = 0;
				short numOfScanners = 0;
				int nScannerCount = 0;
				string outXML = "";
				int[] scannerIdList = new int[MAX_NUM_DEVICES];

				try {
					m_pCoreScanner.GetScanners(out numOfScanners, scannerIdList, out outXML, out status);
					DisplayResult(status, "GET_SCANNERS");
					if (STATUS_SUCCESS == status) {
						m_nTotalScanners = numOfScanners;
						m_xml.ReadXmlString_GetScanners(outXML, m_arScanners, numOfScanners, out nScannerCount);
						for (int index = 0; index < m_arScanners.Length; index++) 
							for (int i = 0; i < claimlist.Count; i++) 
								if (string.Compare(claimlist[i], m_arScanners[index].SERIALNO) == 0) {
									Scanner objScanner = (Scanner)m_arScanners.GetValue(index);
									objScanner.CLAIMED = true;
								}

						FillScannerList();
						UpdateOutXml(outXML);

						for (int index = 0; index < m_nTotalScanners; index++) {
							Scanner objScanner = (Scanner)m_arScanners.GetValue(index);
							string[] strItems = new string[] { "", "", "", "", "" };

							inXml = "<inArgs><scannerID>" + objScanner.SCANNERID + "</scannerID></inArgs>";

							for (int i = 0; i < claimlist.Count; i++) 
								if (string.Compare(claimlist[i], objScanner.SERIALNO) == 0) 
									ExecCmd(opCode, ref inXml, out outXml, out status);
						}

						return m_nTotalScanners > 0;
					}
				} catch (Exception ex) {
					MessageBox.Show("Error GETSCANNERS - " + ex.Message, APP_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return false;
		}

		private void UpdateOutXml(string strOut) {
			txtOutXml.Dispatcher.Invoke(() => {
				txtOutXml.Text = IndentXmlString(strOut);
			});

			//if (txtOutXml.InvokeRequired) {
			//	txtOutXml.Invoke(new MethodInvoker(delegate {
			//		txtOutXml.Text = IndentXmlString(strOut);
			//	}));
			//} else {
				//txtOutXml.Text = IndentXmlString(strOut);
			//}
		}

		private static string IndentXmlString(string strXml) {
			string outXml = string.Empty;
			MemoryStream ms = new MemoryStream();
			// Create a XMLTextWriter that will send its output to a memory stream (file)
			XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode);
			XmlDocument doc = new XmlDocument();

			try {
				// Load the unformatted XML text string into an instance
				// of the XML Document Object Model (DOM)
				doc.LoadXml(strXml);

				// Set the formatting property of the XML Text Writer to indented
				// the text writer is where the indenting will be performed
				xtw.Formatting = Formatting.Indented;

				// write dom xml to the xmltextwriter
				doc.WriteContentTo(xtw);
				// Flush the contents of the text writer
				// to the memory stream, which is simply a memory file
				xtw.Flush();

				// set to start of the memory stream (file)
				ms.Seek(0, SeekOrigin.Begin);
				// create a reader to read the contents of
				// the memory stream (file)
				StreamReader sr = new StreamReader(ms);
				// return the formatted string to caller
				return sr.ReadToEnd();
			} catch (Exception) {
				return string.Empty;
			}
		}

		private void FillScannerList() {
			lstvScanners.Dispatcher.Invoke(() => {
				lstvScanners.Items.Clear();
			});

			InitScannersCount();

			for (int index = 0; index < m_nTotalScanners; index++) {
				Scanner objScanner = (Scanner)m_arScanners.GetValue(index);
				ScannerItem scannerItem = new ScannerItem {
					Number = objScanner.SCANNERID,
					ComInterface = objScanner.SCANNERTYPE,
					Model = objScanner.MODELNO,
					Firmware = objScanner.SCANNERFIRMWARE,
					Built = objScanner.SCANNERMNFDATE,
					SerialOrPort = objScanner.SERIALNO,
					GuID = objScanner.GUID
				};

				switch (objScanner.SCANNERTYPE) {
					case Scanner.SCANNER_SNAPI:
						m_nArTotalScannersInType[0]++;
						break;

					case Scanner.SCANNER_SSI:
						m_nArTotalScannersInType[1]++;
						break;

					case Scanner.SCANNER_IBMHID:
						m_nArTotalScannersInType[2]++;
						scannerItem.ComInterface = "IBM HANDHELD";
						break;

					case Scanner.SCANNER_OPOS:
						m_nArTotalScannersInType[2]++;
						scannerItem.ComInterface = "USB OPOS";
						break;

					case Scanner.SCANNER_NIXMODB:
						m_nArTotalScannersInType[3]++;
						break;

					case Scanner.SCANNER_HIDKB:
						m_nArTotalScannersInType[4]++;
						scannerItem.ComInterface = "HID KEYBOARD";
						break;

					case Scanner.SCANNER_IBMTT:
						m_nArTotalScannersInType[5]++;
						scannerItem.ComInterface = "IBM TABLETOP";
						break;

					case Scanner.SCALE_IBM:
						m_nArTotalScannersInType[6]++;
						scannerItem.ComInterface = "IBM SCALE";
						break;

					case Scanner.SCANNER_SSI_BT:
						m_nArTotalScannersInType[6]++;
						break;
				}
				lstvScanners.Dispatcher.Invoke(() => {
					lstvScanners.Items.Add(scannerItem);
				});
			}

			UpdateScannerCountLabels();
		}
	}
}
