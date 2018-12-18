using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZebraBarcodeScan {
	class FirebirdClient {
		private FbConnection connection;

		public FirebirdClient(string ipAddress, string baseName, string user, string pass) {
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder {
				DataSource = ipAddress,
				Database = baseName,
				UserID = user,
				Password = pass,
				Charset = "NONE",
				Pooling = false
			};

			connection = new FbConnection(cs.ToString());
			//IsConnectionOpened();
		}

		public void Close() {
			connection.Close();
		}

		private bool IsConnectionOpened() {
			if (connection.State != ConnectionState.Open) {
				try {
					connection.Open();
				} catch (Exception e) {
					MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка подключения к БД",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return connection.State == ConnectionState.Open;
		}

		public DataTable GetDataTable(string query, Dictionary<string, string> parameters) {
			DataTable dataTable = new DataTable();

			if (!IsConnectionOpened())
				return dataTable;

			try {
				FbCommand command = new FbCommand(query, connection);

				if (parameters.Count > 0) {
					foreach (KeyValuePair<string, string> parameter in parameters)
						command.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}

				FbDataAdapter fbDataAdapter = new FbDataAdapter(command);
				fbDataAdapter.Fill(dataTable);
			} catch (Exception e) {
				MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка выполнения запроса к БД",
					MessageBoxButton.OK, MessageBoxImage.Error);
				connection.Close();
			}

			return dataTable;
		}

		public bool ExecuteUpdateQuery(string query, Dictionary<string, object> parameters) {
			bool updated = false;

			if (!IsConnectionOpened())
				return updated;

			try {
				FbCommand update = new FbCommand(query, connection);

				if (parameters.Count > 0) {
					foreach (KeyValuePair<string, object> parameter in parameters)
						update.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}

				updated = update.ExecuteNonQuery() > 0 ? true : false;
			} catch (Exception e) {
				MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Ошибка выполнения запроса к БД",
					MessageBoxButton.OK, MessageBoxImage.Error);
				connection.Close();
			}

			return updated;
		}
	}
}