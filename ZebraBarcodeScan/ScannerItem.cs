using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraBarcodeScan {
	class ScannerItem {
		public string Number { get; set; }
		public string ComInterface { get; set; }
		public string Model { get; set; }
		public string Firmware { get; set; }
		public string Built { get; set; }
		public string SerialOrPort { get; set; }
		public string GuID { get; set; }
	}
}
