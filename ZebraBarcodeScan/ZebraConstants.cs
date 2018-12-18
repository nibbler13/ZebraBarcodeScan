using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZebraBarcodeScan {
	public partial class MainWindow : Window {
		// Total number of scanner types
		public const short TOTAL_SCANNER_TYPES = SCALE_TYPES_SSI_BT;

		// Symbology types 
		const int ST_NOT_APP = 0x00;
		const int ST_CODE_39 = 0x01;
		const int ST_CODABAR = 0x02;
		const int ST_CODE_128 = 0x03;
		const int ST_D2OF5 = 0x04;
		const int ST_IATA = 0x05;
		const int ST_I2OF5 = 0x06;
		const int ST_CODE93 = 0x07;
		const int ST_UPCA = 0x08;
		const int ST_UPCE0 = 0x09;
		const int ST_EAN8 = 0x0a;
		const int ST_EAN13 = 0x0b;
		const int ST_CODE11 = 0x0c;
		const int ST_CODE49 = 0x0d;
		const int ST_MSI = 0x0e;
		const int ST_EAN128 = 0x0f;
		const int ST_UPCE1 = 0x10;
		const int ST_PDF417 = 0x11;
		const int ST_CODE16K = 0x12;
		const int ST_C39FULL = 0x13;
		const int ST_UPCD = 0x14;
		const int ST_TRIOPTIC = 0x15;
		const int ST_BOOKLAND = 0x16;
		const int ST_COUPON = 0x17;
		const int ST_NW7 = 0x18;
		const int ST_ISBT128 = 0x19;
		const int ST_MICRO_PDF = 0x1a;
		const int ST_DATAMATRIX = 0x1b;
		const int ST_QR_CODE = 0x1c;
		const int ST_MICRO_PDF_CCA = 0x1d;
		const int ST_POSTNET_US = 0x1e;
		const int ST_PLANET_CODE = 0x1f;
		const int ST_CODE_32 = 0x20;
		const int ST_ISBT128_CON = 0x21;
		const int ST_JAPAN_POSTAL = 0x22;
		const int ST_AUS_POSTAL = 0x23;
		const int ST_DUTCH_POSTAL = 0x24;
		const int ST_MAXICODE = 0x25;
		const int ST_CANADIN_POSTAL = 0x26;
		const int ST_UK_POSTAL = 0x27;
		const int ST_MACRO_PDF = 0x28;
		const int ST_MACRO_QR_CODE = 0x29;
		const int ST_MICRO_QR_CODE = 0x2c;
		const int ST_AZTEC = 0x2d;
		const int ST_AZTEC_RUNE = 0x2e;
		const int ST_DISTANCE = 0x2f;
		const int ST_GS1_DATABAR = 0x30;
		const int ST_GS1_DATABAR_LIMITED = 0x31;
		const int ST_GS1_DATABAR_EXPANDED = 0x32;
		const int ST_PARAMETER = 0x33;
		const int ST_USPS_4CB = 0x34;
		const int ST_UPU_FICS_POSTAL = 0x35;
		const int ST_ISSN = 0x36;
		const int ST_SCANLET = 0x37;
		const int ST_CUECODE = 0x38;
		const int ST_MATRIX2OF5 = 0x39;
		const int ST_UPCA_2 = 0x48;
		const int ST_UPCE0_2 = 0x49;
		const int ST_EAN8_2 = 0x4a;
		const int ST_EAN13_2 = 0x4b;
		const int ST_UPCE1_2 = 0x50;
		const int ST_CCA_EAN128 = 0x51;
		const int ST_CCA_EAN13 = 0x52;
		const int ST_CCA_EAN8 = 0x53;
		const int ST_CCA_RSS_EXPANDED = 0x54;
		const int ST_CCA_RSS_LIMITED = 0x55;
		const int ST_CCA_RSS14 = 0x56;
		const int ST_CCA_UPCA = 0x57;
		const int ST_CCA_UPCE = 0x58;
		const int ST_CCC_EAN128 = 0x59;
		const int ST_TLC39 = 0x5A;
		const int ST_CCB_EAN128 = 0x61;
		const int ST_CCB_EAN13 = 0x62;
		const int ST_CCB_EAN8 = 0x63;
		const int ST_CCB_RSS_EXPANDED = 0x64;
		const int ST_CCB_RSS_LIMITED = 0x65;
		const int ST_CCB_RSS14 = 0x66;
		const int ST_CCB_UPCA = 0x67;
		const int ST_CCB_UPCE = 0x68;
		const int ST_SIGNATURE_CAPTURE = 0x69;
		const int ST_MOA = 0x6A;
		const int ST_PDF417_PARAMETER = 0x70;
		const int ST_CHINESE2OF5 = 0x72;
		const int ST_KOREAN_3_OF_5 = 0x73;
		const int ST_DATAMATRIX_PARAM = 0x74;
		const int ST_CODE_Z = 0x75;
		const int ST_UPCA_5 = 0x88;
		const int ST_UPCE0_5 = 0x89;
		const int ST_EAN8_5 = 0x8a;
		const int ST_EAN13_5 = 0x8b;
		const int ST_UPCE1_5 = 0x90;
		const int ST_MACRO_MICRO_PDF = 0x9A;
		const int ST_OCRB = 0xA0;
		const int ST_OCR = 0xA1;
		const int ST_PARSED_DRIVER_LICENSE = 0xB1;
		const int ST_PARSED_UID = 0xB2;
		const int ST_PARSED_NDC = 0xB3;
		const int ST_DATABAR_COUPON = 0xB4;
		const int ST_PARSED_XML = 0xB6;
		const int ST_HAN_XIN_CODE = 0xB7;
		const int ST_CALIBRATION = 0xC0;
		const int ST_GS1_DATAMATRIX = 0xC1;
		const int ST_GS1_QR = 0xC2;
		const int BT_MAINMARK = 0xC3;
		const int BT_DOTCODE = 0xC4;
		const int BT_GRID_MATRIX = 0xC8;


		// Scanner types
		public const short SCANNER_TYPES_ALL = 1;
		public const short SCANNER_TYPES_SNAPI = 2;
		public const short SCANNER_TYPES_SSI = 3;
		public const short SCANNER_TYPES_RSM = 4;
		public const short SCANNER_TYPES_IMAGING = 5;
		public const short SCANNER_TYPES_IBMHID = 6;
		public const short SCANNER_TYPES_NIXMODB = 7;
		public const short SCANNER_TYPES_HIDKB = 8;
		public const short SCANNER_TYPES_IBMTT = 9;
		public const short SCALE_TYPES_IBM = 10;
		public const short SCALE_TYPES_SSI_BT = 11;

		//End Symbology Types

		const string APP_TITLE = "Scanner Multi-Interface Test Utility";
		const string STR_OPEN = "Start";
		const string STR_CLOSE = "Stop";
		const string STR_REFRESH = "Rediscover Scanners";
		const string STR_FIND = "Discover Scanners";
		const int NUM_SCANNER_EVENTS = 6;


		// available values for 'status' //
		const int STATUS_SUCCESS = 0;
		const int STATUS_FALSE = 1;
		const int STATUS_LOCKED = 10;


		//****** CORESCANNER PROTOCOL ******//
		const int GET_VERSION = 1000;
		const int REGISTER_FOR_EVENTS = 1001;
		const int UNREGISTER_FOR_EVENTS = 1002;
		const int GET_PAIRING_BARCODE = 1005;   // Get  Blue tooth scanner pairing bar code
		const int CLAIM_DEVICE = 1500;
		const int RELEASE_DEVICE = 1501;
		const int ABORT_MACROPDF = 2000;
		const int ABORT_UPDATE_FIRMWARE = 2001;
		const int DEVICE_AIM_OFF = 2002;
		const int DEVICE_AIM_ON = 2003;
		const int FLUSH_MACROPDF = 2005;
		const int GET_ALL_PARAMETERS = 2006;
		const int GET_PARAMETERS = 2007;
		const int DEVICE_GET_SCANNER_CAPABILITIES = 2008;
		const int DEVICE_LED_OFF = 2009;
		const int DEVICE_LED_ON = 2010;
		const int DEVICE_PULL_TRIGGER = 2011;
		const int DEVICE_RELEASE_TRIGGER = 2012;
		const int DEVICE_SCAN_DISABLE = 2013;
		const int DEVICE_SCAN_ENABLE = 2014;
		const int SET_PARAMETER_DEFAULTS = 2015;
		const int DEVICE_SET_PARAMETERS = 2016;
		const int SET_PARAMETER_PERSISTANCE = 2017;
		const int DEVICE_BEEP_CONTROL = 2018;
		const int REBOOT_SCANNER = 2019;
		const int DISCONNECT_BT_SCANNER = 2023;
		const int DEVICE_CAPTURE_IMAGE = 3000;
		const int ABORT_IMAGE_XFER = 3001;
		const int DEVICE_CAPTURE_BARCODE = 3500;
		const int DEVICE_CAPTURE_VIDEO = 4000;
		public const int RSM_ATTR_GETALL = 5000;
		public const int RSM_ATTR_GET = 5001;
		public const int RSM_ATTR_GETNEXT = 5002;
		public const int RSM_ATTR_SET = 5004;
		public const int RSM_ATTR_STORE = 5005;
		const int GET_DEVICE_TOPOLOGY = 5006;
		const int START_NEW_FIRMWARE = 5014;
		const int UPDATE_ATTRIB_META_FILE = 5015;
		const int UPDATE_FIRMWARE = 5016;
		const int UPDATE_FIRMWARE_FROM_PLUGIN = 5017;
		const int UPDATE_DECODE_TONE = 5050;
		const int ERASE_DECODE_TONE = 5051;
		const int SET_ACTION = 6000;

		const int KEYBOARD_EMULATOR_ENABLE = 6300;//6300
		const int KEYBOARD_EMULATOR_SET_LOCALE = 6301;  //6301
		const int KEYBOARD_EMULATOR_GET_CONFIG = 6302;  //6302

		const int CONFIGURE_DADF = 6400;
		const int RESET_DADF = 6401;

		const int MAX_NUM_DEVICES = 255;/* Maximum number of scanners to be connected*/

		const int SUBSCRIBE_BARCODE = 1;
		const int SUBSCRIBE_IMAGE = 2;
		const int SUBSCRIBE_VIDEO = 4;
		const int SUBSCRIBE_RMD = 8;
		const int SUBSCRIBE_PNP = 16;
		const int SUBSCRIBE_OTHER = 32;
		
		private string GetSymbology(int Code) {
			switch (Code) {
				case ST_NOT_APP:
					return "NOT APPLICABLE";
				case ST_CODE_39:
					return "CODE 39";
				case ST_CODABAR:
					return "CODABAR";
				case ST_CODE_128:
					return "CODE 128";
				case ST_D2OF5:
					return "DISCRETE 2 OF 5";
				case ST_IATA:
					return "IATA";
				case ST_I2OF5:
					return "INTERLEAVED 2 OF 5";
				case ST_CODE93:
					return "CODE 93";
				case ST_UPCA:
					return "UPC-A";
				case ST_UPCE0:
					return "UPC-E0";
				case ST_EAN8:
					return "EAN-8";
				case ST_EAN13:
					return "EAN-13";
				case ST_CODE11:
					return "CODE 11";
				case ST_CODE49:
					return "CODE 49";
				case ST_MSI:
					return "MSI";
				case ST_EAN128:
					return "EAN-128";
				case ST_UPCE1:
					return "UPC-E1";
				case ST_PDF417:
					return "PDF-417";
				case ST_CODE16K:
					return "CODE 16K";
				case ST_C39FULL:
					return "CODE 39 FULL ASCII";
				case ST_UPCD:
					return "UPC-D";
				case ST_TRIOPTIC:
					return "CODE 39 TRIOPTIC";
				case ST_BOOKLAND:
					return "BOOKLAND";
				case ST_COUPON:
					return "COUPON CODE";
				case ST_NW7:
					return "NW-7";
				case ST_ISBT128:
					return "ISBT-128";
				case ST_MICRO_PDF:
					return "MICRO PDF";
				case ST_DATAMATRIX:
					return "DATAMATRIX";
				case ST_QR_CODE:
					return "QR CODE";
				case ST_MICRO_PDF_CCA:
					return "MICRO PDF CCA";
				case ST_POSTNET_US:
					return "POSTNET US";
				case ST_PLANET_CODE:
					return "PLANET CODE";
				case ST_CODE_32:
					return "CODE 32";
				case ST_ISBT128_CON:
					return "ISBT-128 CON";
				case ST_JAPAN_POSTAL:
					return "JAPAN POSTAL";
				case ST_AUS_POSTAL:
					return "AUS POSTAL";
				case ST_DUTCH_POSTAL:
					return "DUTCH POSTAL";
				case ST_MAXICODE:
					return "MAXICODE";
				case ST_CANADIN_POSTAL:
					return "CANADIAN POSTAL";
				case ST_UK_POSTAL:
					return "UK POSTAL";
				case ST_MACRO_PDF:
					return "MACRO PDF";
				case ST_MACRO_QR_CODE:
					return "MACRO QR CODE";
				case ST_MICRO_QR_CODE:
					return "MICRO QR CODE";
				case ST_AZTEC:
					return "AZTEC";
				case ST_AZTEC_RUNE:
					return "AZTEC RUNE";
				case ST_DISTANCE:
					return "DISTANCE";
				case ST_GS1_DATABAR:
					return "GS1 DATABAR";
				case ST_GS1_DATABAR_LIMITED:
					return "GS1 DATABAR LIMITED";
				case ST_GS1_DATABAR_EXPANDED:
					return "GS1 DATABAR EXPANDED";
				case ST_PARAMETER:
					return "PARAMETER";
				case ST_USPS_4CB:
					return "USPS 4CB";
				case ST_UPU_FICS_POSTAL:
					return "UPU FICS POSTAL";
				case ST_ISSN:
					return "ISSN";
				case ST_SCANLET:
					return "SCANLET";
				case ST_CUECODE:
					return "CUECODE";
				case ST_MATRIX2OF5:
					return "MATRIX 2 OF 5";
				case ST_UPCA_2:
					return "UPC-A + 2 SUPPLEMENTAL";
				case ST_UPCE0_2:
					return "UPC-E0 + 2 SUPPLEMENTAL";
				case ST_EAN8_2:
					return "EAN-8 + 2 SUPPLEMENTAL";
				case ST_EAN13_2:
					return "EAN-13 + 2 SUPPLEMENTAL";
				case ST_UPCE1_2:
					return "UPC-E1 + 2 SUPPLEMENTAL";
				case ST_CCA_EAN128:
					return "CCA EAN-128";
				case ST_CCA_EAN13:
					return "CCA EAN-13";
				case ST_CCA_EAN8:
					return "CCA EAN-8";
				case ST_CCA_RSS_EXPANDED:
					return "GS1 DATABAR EXPANDED COMPOSITE (CCA)";
				case ST_CCA_RSS_LIMITED:
					return "GS1 DATABAR LIMITED COMPOSITE (CCA)";
				case ST_CCA_RSS14:
					return "GS1 DATABAR COMPOSITE (CCA)";
				case ST_CCA_UPCA:
					return "CCA UPC-A";
				case ST_CCA_UPCE:
					return "CCA UPC-E";
				case ST_CCC_EAN128:
					return "CCA EAN-128";
				case ST_TLC39:
					return "TLC-39";
				case ST_CCB_EAN128:
					return "CCB EAN-128";
				case ST_CCB_EAN13:
					return "CCB EAN-13";
				case ST_CCB_EAN8:
					return "CCB EAN-8";
				case ST_CCB_RSS_EXPANDED:
					return "GS1 DATABAR EXPANDED COMPOSITE (CCB)";
				case ST_CCB_RSS_LIMITED:
					return "GS1 DATABAR LIMITED COMPOSITE (CCB)";
				case ST_CCB_RSS14:
					return "GS1 DATABAR COMPOSITE (CCB)";
				case ST_CCB_UPCA:
					return "CCB UPC-A";
				case ST_CCB_UPCE:
					return "CCB UPC-E";
				case ST_SIGNATURE_CAPTURE:
					return "SIGNATURE CAPTUREE";
				case ST_MOA:
					return "MOA";
				case ST_PDF417_PARAMETER:
					return "PDF417 PARAMETER";
				case ST_CHINESE2OF5:
					return "CHINESE 2 OF 5";
				case ST_KOREAN_3_OF_5:
					return "KOREAN 3 OF 5";
				case ST_DATAMATRIX_PARAM:
					return "DATAMATRIX PARAM";
				case ST_CODE_Z:
					return "CODE Z";
				case ST_UPCA_5:
					return "UPC-A + 5 SUPPLEMENTAL";
				case ST_UPCE0_5:
					return "UPC-E0 + 5 SUPPLEMENTAL";
				case ST_EAN8_5:
					return "EAN-8 + 5 SUPPLEMENTAL";
				case ST_EAN13_5:
					return "EAN-13 + 5 SUPPLEMENTAL";
				case ST_UPCE1_5:
					return "UPC-E1 + 5 SUPPLEMENTAL";
				case ST_MACRO_MICRO_PDF:
					return "MACRO MICRO PDF";
				case ST_OCRB:
					return "OCRB";
				case ST_OCR:
					return "OCR";
				case ST_PARSED_DRIVER_LICENSE:
					return "PARSED DRIVER LICENSE";
				case ST_PARSED_UID:
					return "PARSED UID";
				case ST_PARSED_NDC:
					return "PARSED NDC";
				case ST_DATABAR_COUPON:
					return "DATABAR COUPON";
				case ST_PARSED_XML:
					return "PARSED XML";
				case ST_HAN_XIN_CODE:
					return "HAN XIN CODE";
				case ST_CALIBRATION:
					return "CALIBRATION";
				case ST_GS1_DATAMATRIX:
					return "GS1 DATA MATRIX";
				case ST_GS1_QR:
					return "GS1 QR";
				case BT_MAINMARK:
					return "MAIL MARK";
				case BT_DOTCODE:
					return "DOT CODE";
				case BT_GRID_MATRIX:
					return "GRID MATRIX";
				default:
					return "";
			}
		}
	}
}
