
// F:\audio\ESS_SuiteFW.exe
// ESS_SuiteFW, Version=1.0.0.10, Culture=neutral, PublicKeyToken=null
// Global type: <Module>
// Entry point: ESS_SuiteFW.Program.Main
// Architecture: AnyCPU (32-bit preferred)
// Runtime: v4.0.30319
// This assembly was compiled using the /deterministic option.
// Hash algorithm: SHA1
// Debug info: Loaded from PDB file: F:\audio\ESS_SuiteFW.pdb

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESS_SuiteFW.Extensions;
using ESS_SuiteFW.Interfaces;
using ESS_SuiteFW.Model;
using ESS_SuiteFW.Types;
using LibUsbDotNet.DeviceNotify;
using Microsoft.Win32.SafeHandles;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("ESS_SuiteFW")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ESS_SuiteFW")]
[assembly: AssemblyCopyright("Copyright ©  2020")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("e97fe0eb-3bbd-4106-b72b-18896ade5670")]
[assembly: AssemblyFileVersion("1.0.0.10")]
[assembly: TargetFramework(".NETFramework,Version=v4.7.2", FrameworkDisplayName = ".NET Framework 4.7.2")]
[assembly: AssemblyVersion("1.0.0.10")]
namespace ESS_SuiteFW
{
	public class Program
	{
		private static void Main(string[] args)
		{
			int exitCode = 0;
			string text = "";
			while (text.Equals(""))
			{
				Task<string> task = Task.Run(() => Upload());
				task.Wait();
				text = task.Result;
				if (text.Contains("ESS014"))
				{
					exitCode = (text.Contains("Success") ? 1 : 2);
				}
				else if (text.Contains("ESS081"))
				{
					string[] array = text.Split(':');
					DialogResult dialogResult = MessageBox.Show(array[1] ?? "", "ESS_SuiteFW Update Complete", MessageBoxButtons.OK);
					if (dialogResult == DialogResult.OK && array[1].Contains("Failed"))
					{
						exitCode = 1;
					}
				}
				else if (text.Contains("Exit"))
				{
					exitCode = 0;
				}
				else if (text.Equals("Error"))
				{
					exitCode = 1;
				}
			}
			Environment.Exit(exitCode);
		}

		public static async Task<string> Upload()
		{
			I2CRegisterModel _i2CRegisterModel = new I2CRegisterModel();
			return await _i2CRegisterModel.Upload();
		}
	}
}
namespace ESS_SuiteFW.Types
{
	public enum ChipMode
	{
		ESS014,
		ESS071,
		ESS057,
		ESS079
	}
}
namespace ESS_SuiteFW.Model
{
	public abstract class DigitalUsbBase
	{
		protected int VID;

		protected int PID;

		protected bool connected = false;

		public bool Connected => connected;

		public abstract void CloseDevice();

		public abstract bool SendControlData(byte[] buffer);

		public abstract bool ReceiveControlData(byte[] buffer);

		public abstract bool ReceiveInterruptData(byte[] buffer);
	}
	public class DigitalUsbHidModel
	{
		public struct HIDD_ATTRIBUTES
		{
			internal uint Size;

			internal ushort VendorID;

			internal ushort ProductID;

			internal ushort VersionNumber;
		}

		internal struct SP_DEVICE_INTERFACE_DATA
		{
			internal int cbSize;

			internal Guid InterfaceClassGuid;

			internal uint Flags;

			internal uint Reserved;
		}

		public struct HIDP_CAPS
		{
			internal ushort Usage;

			internal ushort UsagePage;

			internal ushort InputReportByteLength;

			internal ushort OutputReportByteLength;

			internal ushort FeatureReportByteLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			internal ushort[] Reserved;

			internal ushort NumberLinkCollectionNodes;

			internal ushort NumberInputButtonCaps;

			internal ushort NumberInputValueCaps;

			internal ushort NumberInputDataIndices;

			internal ushort NumberOutputButtonCaps;

			internal ushort NumberOutputValueCaps;

			internal ushort NumberOutputDataIndices;

			internal ushort NumberFeatureButtonCaps;

			internal ushort NumberFeatureValueCaps;

			internal ushort NumberFeatureDataIndices;
		}

		internal SafeFileHandle deviceHandle;

		internal SafeFileHandle deviceDeviceHandle;

		internal SafeFileHandle deviceReadHandle;

		internal SafeFileHandle deviceWriteHandle;

		private HIDP_CAPS Capabilities;

		public bool HidMerged;

		private const int DIGCF_DEFAULT = 1;

		private const int DIGCF_PRESENT = 2;

		private const int DIGCF_ALLCLASSES = 4;

		private const int DIGCF_PROFILE = 8;

		private const int DIGCF_INTERFACEDEVICE = 16;

		private const uint FILE_SHARE_READ = 1u;

		private const uint FILE_SHARE_WRITE = 2u;

		private const uint GENERIC_READ = 2147483648u;

		private const uint GENERIC_WRITE = 1073741824u;

		private const uint OPEN_EXISTING = 3u;

		private const uint FILE_FLAG_OVERLAPPED = 1073741824u;

		[DllImport("hid.dll", SetLastError = true)]
		private static extern void HidD_GetHidGuid(out Guid hidGuid);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_GetInputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_SetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_GetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern bool ReadFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_GetAttributes(int HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_GetPreparsedData(int HidDeviceObject, ref int PPreParsedData);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidP_GetCaps(int PPreParsedData, ref HIDP_CAPS PHidCaps);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern bool HidD_FreePreparsedData(ref int PPreParsedData);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern int SetupDiCreateDeviceInfoList(ref Guid ClassGuid, int hwndParent);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, uint DeviceInterfaceDetailDataSize, ref uint RequiredSize, IntPtr DeviceInfoData);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool UnregisterDeviceNotification(IntPtr Handle);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr SecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		public bool SendData(byte[] buffer)
		{
			if (buffer.Length > 64)
			{
				throw new Exception("Can only send 64 bytes at a time via the HID device.");
			}
			byte[] array = new byte[65];
			for (int i = 0; i < buffer.Length; i++)
			{
				array[i + 1] = buffer[i];
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			bool result = ((!HidMerged) ? HidD_SetOutputReport((int)deviceWriteHandle.DangerousGetHandle(), ref array[0], 65) : HidD_SetFeature((int)deviceDeviceHandle.DangerousGetHandle(), ref array[0], 65));
			gCHandle.Free();
			return result;
		}

		public bool ReceiveData(byte[] buffer)
		{
			byte[] array = new byte[65];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			bool result = ((!HidMerged) ? HidD_GetInputReport((int)deviceReadHandle.DangerousGetHandle(), ref array[0], 65) : HidD_GetFeature((int)deviceDeviceHandle.DangerousGetHandle(), ref array[0], 65));
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = array[i + 1];
			}
			gCHandle.Free();
			return result;
		}

		public bool ReceiveData_Interrupt(byte[] buffer)
		{
			byte[] array = new byte[65];
			int lpNumberOfBytesRead = 0;
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			GCHandle gCHandle2 = GCHandle.Alloc(lpNumberOfBytesRead, GCHandleType.Pinned);
			bool result = ReadFile((int)deviceReadHandle.DangerousGetHandle(), ref array[0], 65, ref lpNumberOfBytesRead, IntPtr.Zero);
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = array[i + 1];
			}
			gCHandle.Free();
			gCHandle2.Free();
			return result;
		}

		public void DisconnectHID()
		{
			if (!deviceHandle.IsInvalid && !deviceHandle.IsClosed)
			{
				deviceHandle.Close();
			}
			if (!deviceReadHandle.IsInvalid && !deviceReadHandle.IsClosed)
			{
				deviceReadHandle.Close();
			}
			if (!deviceWriteHandle.IsInvalid && !deviceWriteHandle.IsClosed)
			{
				deviceWriteHandle.Close();
			}
		}

		public bool ConnectHID(int VendorID, int ProductID, int nIf)
		{
			HIDD_ATTRIBUTES hIDD_ATTRIBUTES = default;
			SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = default;
			bool flag = false;
			int num = 0;
			bool flag2 = false;
			uint RequiredSize = 0u;
			IntPtr zero = IntPtr.Zero;
			HidD_GetHidGuid(out var hidGuid);
			IntPtr deviceInfoSet = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, 18);
			DeviceInterfaceData.cbSize = Marshal.SizeOf(DeviceInterfaceData);
			num = 0;
			flag = false;
			string text = ((nIf >= 0) ? $"vid_{VendorID:x4}&pid_{ProductID:x4}&mi_{nIf:x2}" : $"vid_{VendorID:x4}&pid_{ProductID:x4}");
			do
			{
				if (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref hidGuid, num, ref DeviceInterfaceData))
				{
					bool flag3 = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0u, ref RequiredSize, IntPtr.Zero);
					zero = Marshal.AllocHGlobal((int)RequiredSize);
					Marshal.WriteInt32(zero, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);
					uint RequiredSize2 = 0u;
					flag3 = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, zero, RequiredSize, ref RequiredSize2, IntPtr.Zero);
					IntPtr ptr = new IntPtr((long)zero + 4);
					string text2 = Marshal.PtrToStringAuto(ptr);
					string text3 = text2.Substring(8, (nIf < 0) ? 17 : 23);
					if (text3 == text)
					{
						deviceHandle = CreateFile(text2, 0u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
						int PPreParsedData = 0;
						HidD_GetPreparsedData((int)deviceHandle.DangerousGetHandle(), ref PPreParsedData);
						HidP_GetCaps(PPreParsedData, ref Capabilities);
						HidD_FreePreparsedData(ref PPreParsedData);
						if (Capabilities.OutputReportByteLength != 65 && Capabilities.FeatureReportByteLength != 65)
						{
							num++;
							continue;
						}
						if (Capabilities.OutputReportByteLength == 65)
						{
							HidMerged = false;
						}
						else if (Capabilities.FeatureReportByteLength == 65)
						{
							HidMerged = true;
						}
						flag2 = true;
						deviceWriteHandle = CreateFile(text2, 1073741824u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
						deviceReadHandle = CreateFile(text2, 2147483648u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
						deviceDeviceHandle = CreateFile(text2, 0u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
					}
					Marshal.FreeHGlobal(zero);
				}
				else
				{
					flag = true;
				}
				num++;
			}
			while (!flag && !flag2);
			SetupDiDestroyDeviceInfoList(deviceInfoSet);
			return flag2;
		}
	}
	public class DigitalUsbModel : DigitalUsbBase
	{
		private DigitalUsbHidModel _usbHidModel = new DigitalUsbHidModel();

		public DigitalUsbModel(int vendorID, int productID, int nIf = -1)
		{
			VID = vendorID;
			PID = productID;
			Reconnect(vendorID, productID, nIf);
		}

		public bool Reconnect(int Vid, int Pid, int nIf)
		{
			connected = _usbHidModel.ConnectHID(Vid, Pid, nIf);
			return connected;
		}

		public override bool ReceiveControlData(byte[] buffer)
		{
			if (!connected)
			{
				return false;
			}
			return _usbHidModel.ReceiveData(buffer);
		}

		public override bool ReceiveInterruptData(byte[] buffer)
		{
			if (!connected)
			{
				return false;
			}
			return _usbHidModel.ReceiveData_Interrupt(buffer);
		}

		public override bool SendControlData(byte[] buffer)
		{
			if (!connected)
			{
				return false;
			}
			return _usbHidModel.SendData(buffer);
		}

		public override void CloseDevice()
		{
			if (connected)
			{
				_usbHidModel.DisconnectHID();
				connected = false;
			}
		}

		public int GetPID()
		{
			return PID;
		}

		public int GetVID()
		{
			return VID;
		}
	}
	public class I2CRegisterModel : IUsbDevice
	{
		public const int NUM_OF_REGISTERS = 256;

		public const int VID_ESS = 1173;

		public const int PID_ESS_71 = 12288;

		public const int PID_ESS_81_DEFAULT = 12289;

		public const int PID_ESS_14 = 4096;

		public const int PID_ESS_14U = 4352;

		public const int VID_ASUS = 2821;

		public const int PID_ASUS_GD90_FS = 6568;

		public const int PID_ASUS_GD90_HS = 6491;

		public const int PID_ASUS_RH200 = 6467;

		public const int EEPROM_CONFIG_BITS_ADDRESS = 32672;

		public static int VID = 1173;

		public static int PID;

		public const int TRANSFER_SIZE = 64;

		private static IDeviceNotifier _usbDeviceNotifier;

		private bool _transactionFailed = false;

		private int _current_PID = 0;

		private int _current_VID = 1173;

		public bool Connected = false;

		private int progress = 0;

		private string previousVersion = "";

		private string chipVersion = "";

		private const string NO_USB_DETECTED = "Device not detected. Please insure you have the device plugged in";

		private const string USB_TRANSACTION_FAILED = "ERROR - USB Transaction Failed";

		public DigitalUsbBase UsbDevice { get; set; }

		public byte DeviceAddress { get; set; } = byte.MaxValue;

		public string USBMode
		{
			get
			{
				byte data = 0;
				ReadRegister_USBBridge(221, ref data);
				if (data.GetBit(2))
				{
					return "SYNC";
				}
				return "ASYNC";
			}
		}

		public ChipMode ChipMode { get; set; }

		public static string Version
		{
			get
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
				return string.Format(versionInfo.FileVersion ?? "");
			}
		}

		public I2CRegisterModel()
		{
			_usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
			_usbDeviceNotifier.OnDeviceNotify += USBDeviceNotifier_OnDeviceNotify;
			PID = 12288;
		}

		public bool CustomI2CCommad(string command, bool checkWrite = true)
		{
			bool result = true;
			try
			{
				if (!UsbDevice.Connected)
				{
					throw new Exception("Device not detected. Please insure you have the device plugged in");
				}
				int deviceAddress = DeviceAddress;
				command.TrimEnd(' ');
				if (command.Contains(";"))
				{
					command = command.Substring(0, command.IndexOf(";"));
				}
				if (command.Contains("//"))
				{
					command = command.Substring(0, command.IndexOf("//")).TrimEnd(' ');
				}
				string[] array = command.Split(' ');
				if (array.Length == 4)
				{
					DeviceAddress = Convert.ToByte(LogicCoreModel.FromHex(array[1]));
					command = command.Remove(command.IndexOf(' ', 0), array[1].Length + 1);
				}
				string[] array2 = command.Split(' ');
				if (array2.Length > 3)
				{
					throw new Exception("Unkown Command " + command);
				}
				if (array2[0].ToString().ToLower() == "w")
				{
					byte address = LogicCoreModel.FromHex(array2[1]);
					byte data = 0;
					if (array2.Length.Equals(3))
					{
						data = LogicCoreModel.FromHex(array2[2]);
						byte data2 = 0;
						WriteRegister(address, data, 0);
						if (checkWrite)
						{
							ReadRegister(address, ref data2);
							if (data != data2)
							{
								throw new Exception("Failed I2C Write! DevAddr: 0x" + DeviceAddress.ToString("X") + " Addr: 0x" + address.ToString("X") + "(" + address + ") Data: 0x" + data.ToString("X") + " Readback: 0x" + data2.ToString("X"));
							}
						}
						PrintDebugMsg("I2C Write -  DevAddr: 0x" + DeviceAddress.ToString("X") + " Addr: 0x" + address.ToString("X") + "(" + address + ") Data: 0x" + data.ToString("X"));
					}
					else
					{
						ReadRegister(address, ref data);
						PrintDebugMsg("I2C Read -  DevAddr: 0x" + DeviceAddress.ToString("X") + " Addr: 0x" + address.ToString("X") + "(" + address + ") Data: 0x" + data.ToString("X"));
					}
				}
				else
				{
					if (!(array2[0].ToString().ToLower() == "delay"))
					{
						if (array2[0].StartsWith("//") || command == "" || array2[0] == "")
						{
							return true;
						}
						throw new Exception("Unkown Command " + array2[0]);
					}
					if (array2[1].ToString().Contains("us"))
					{
						Thread.Sleep(Convert.ToInt32(array2[1].ToString().Replace("us", "")) / 1000);
					}
					else
					{
						if (!array2[1].ToString().Contains("ms"))
						{
							throw new Exception("Unkown Command " + array2[0] + ", Delays must be formatted as: Delay <val> us or Delay <val> ms");
						}
						Thread.Sleep(Convert.ToInt32(array2[1].ToString().Replace("ms", "")));
					}
				}
				DeviceAddress = Convert.ToByte(deviceAddress);
			}
			catch (Exception ex)
			{
				result = false;
				PrintDebugMsg("ERROR - Command Failed: " + command + ". Exception: " + ex.Message);
			}
			return result;
		}

		public bool ReadRegister(byte address, ref byte data)
		{
			if (DeviceAddress.Equals(byte.MaxValue))
			{
				return ReadRegister_USBBridge(address, ref data);
			}
			return ReadRegister_DUT(address, ref data);
		}

		public bool WriteRegister(byte address, byte data, byte devAddress = 0)
		{
			if (!devAddress.Equals(0))
			{
				return WriteRegister_Generic(address, data, devAddress);
			}
			if (DeviceAddress.Equals(byte.MaxValue))
			{
				return WriteRegister_USBBridge(address, data);
			}
			return WriteRegister_DUT(address, data);
		}

		public bool ReadRegisterEEPROM(uint address, int count, ref byte[] data)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			uint num = address;
			int num2 = count;
			int num3 = 0;
			while (num2 > 0)
			{
				byte b = (byte)((num2 >= 64) ? 64 : ((byte)num2));
				byte[] array = new byte[4];
				byte[] data2 = new byte[b];
				array[0] = 21;
				array[1] = (byte)(num >> 8);
				array[2] = (byte)(num & 0xFF);
				array[3] = b;
				if (!SendControlData(array))
				{
					return false;
				}
				if (!ReceiveControlData(ref data2))
				{
					return false;
				}
				for (int i = 0; i < b; i++)
				{
					data[num3] = data2[i];
					num3++;
				}
				num2 = count - num3;
				num += b;
			}
			return true;
		}

		public bool WriteRegisterEEPROM(uint address, int count, byte[] data, bool SPI = false)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			uint num = address;
			int num2 = count;
			int num3 = 0;
			byte b = 4;
			while (num2 > 0)
			{
				byte b2 = (byte)((num2 >= 64 - b) ? 64 : ((byte)(num2 + b)));
				if (SPI && num >> 8 != num + b2 >> 8)
				{
					b2 = Convert.ToByte(256 - (num & 0xFF) + 4);
				}
				byte[] array = new byte[b2];
				array[0] = 23;
				array[1] = (byte)(num >> 8);
				array[2] = (byte)(num & 0xFF);
				array[3] = (byte)(b2 - b);
				for (int i = 0; i < b2 - b; i++)
				{
					array[4 + i] = data[num3];
					num3++;
				}
				if (!SendControlData(array))
				{
					return false;
				}
				num2 = count - num3;
				num += (uint)(b2 - b);
				if (SPI)
				{
					WaitSPIReady();
				}
			}
			return true;
		}

		public int GetEEPROMConfigValues(uint offset, int byteCount)
		{
			byte[] data = new byte[byteCount];
			ReadRegisterEEPROM(offset + 32672, byteCount, ref data);
			uint num = 0u;
			for (int i = 0; i < byteCount; i++)
			{
				num |= (uint)(data[i] << 8 * i);
			}
			return (short)num;
		}

		public void SetEEPROMConfigValues(uint offset, int byteCount, int value)
		{
			byte[] array = new byte[byteCount];
			for (int i = 0; i < byteCount; i++)
			{
				array[i] = (byte)(value >> 8 * i);
			}
			WriteRegisterEEPROM(offset + 32672, byteCount, array);
			Thread.Sleep(2);
		}

		public bool SendControlData(byte[] data, bool ShowDebugMsg = true)
		{
			try
			{
				if (!UsbDevice.SendControlData(data))
				{
					if (!_transactionFailed)
					{
						throw new Exception(string.Format("{0}-> Op1: 0x{1}, Op2: 0x{2}, Add: {3}, Len: {4}", "ERROR - USB Transaction Failed", data[0].ToString("X"), data[1].ToString("X"), data[2], data[3]));
					}
					return false;
				}
				_transactionFailed = false;
				return true;
			}
			catch (Exception ex)
			{
				ErrorHandler(ex, ShowDebugMsg);
				return false;
			}
		}

		public bool ReceiveControlData(ref byte[] data, bool ShowDebugMsg = true)
		{
			try
			{
				if (!UsbDevice.ReceiveControlData(data))
				{
					if (!_transactionFailed)
					{
						throw new Exception(string.Format("{0}-> Op1: 0x{1}, Op2: 0x{2}, Add: {3}, Len: {4}", "ERROR - USB Transaction Failed", data[0].ToString("X"), data[1].ToString("X"), data[2], data[3]));
					}
					return false;
				}
				_transactionFailed = false;
				return true;
			}
			catch (Exception ex)
			{
				ErrorHandler(ex, ShowDebugMsg);
				return false;
			}
		}

		public async Task<string> Upload()
		{
			try
			{
				string str = await Connect();
				string fileName = "";
				if (str.Equals("Connected"))
				{
					previousVersion = Revision() + Build();
					PrintDebugMsg("Previous Version: " + previousVersion);
					byte[] readBack = new byte[1];
					ReadRegisterEEPROM(5u, 1, ref readBack);
					if (readBack[0] == byte.MaxValue)
					{
						byte[] read = new byte[1];
						ReadRegisterEEPROM(32768u, 1, ref read);
						if (read[0] < 0 || read[0] > 3)
						{
							PrintDebugMsg("ERROR - Invalid firmware. \n");
							fileName = InvalidFirmware();
						}
					}
					if (chipVersion.Contains("Engineering"))
					{
						throw new Exception("Engineering Chip Detected. Firmware not updated.");
					}
					if (chipVersion.Contains("Production"))
					{
						if (previousVersion.Equals("0.0.99"))
						{
							fileName = GetFirmwareFile("ES9260_3.");
						}
						else if (previousVersion.StartsWith("0."))
						{
							fileName = GetFirmwareFile("ES9260_0.");
						}
						else if (previousVersion.StartsWith("1."))
						{
							fileName = GetFirmwareFile("ES9260_1.");
						}
						else if (previousVersion.StartsWith("2."))
						{
							fileName = GetFirmwareFile("ES9260_2.");
						}
						else
						{
							fileName = ((!previousVersion.StartsWith("3.")) ? InvalidFirmware() : GetFirmwareFile("ES9260_3."));
						}
					}
					return await UploadAsync(fileName);
				}
				throw new Exception("Failed to connect");
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				PrintDebugMsg(ex2.Message);
				WriteResults(success: false, ex2.Message);
				return "ESS081: Update firmware failed. No device connected. \n Please connect device and re-try.";
			}
		}

		public string InvalidFirmware()
		{
			string result = "";
			PrintDebugMsg("No firmware found. Please select from the following: \n Enter 0 for Tekcon 48 MHz\n Enter 1 for MEC 48 MHz\n Enter 2 for Bizlink 48 MHz\n Enter 3 for MEC 24 MHz");
			switch (Console.ReadLine())
			{
			case "0":
				result = GetFirmwareFile("ES9260_0.");
				break;
			case "1":
				result = GetFirmwareFile("ES9260_1.");
				break;
			case "2":
				result = GetFirmwareFile("ES9260_2.");
				break;
			case "3":
				result = GetFirmwareFile("ES9260_3.");
				break;
			}
			return result;
		}

		private void USBDeviceNotifier_OnDeviceNotify(object sender, DeviceNotifyEventArgs e)
		{
			if (e.Device == null)
			{
				return;
			}
			PrintDebugMsg(string.Format("Device VID: 0x{0} PID: 0x{1} {2}", e.Device.IdVendor.ToString("X4"), e.Device.IdProduct.ToString("X4"), e.EventType));
			if (e.EventType == EventType.DeviceRemoveComplete && e.Device.IdVendor == _current_VID)
			{
				UsbDevice?.CloseDevice();
			}
			else if (e.EventType == EventType.DeviceArrival && e.Device.IdVendor == VID)
			{
				if (e.Device.IdProduct >= 4096 && e.Device.IdProduct <= 4224)
				{
					PID = 4096;
				}
				else if (e.Device.IdProduct >= 12288 && e.Device.IdProduct <= 12416)
				{
					PID = 12288;
				}
				if (UsbDevice == null || !UsbDevice.Connected)
				{
					Thread.Sleep(1000);
					Connect();
				}
			}
		}

		public async Task<string> Connect()
		{
			bool connected = false;
			if (UsbDevice != null && UsbDevice.Connected)
			{
				connected = true;
			}
			else
			{
				PrintDebugMsg("Connecting...");
				for (int i = 0; i < 128; i++)
				{
					UsbDevice = new DigitalUsbModel(VID, PID + i);
					if (UsbDevice.Connected)
					{
						connected = true;
						_current_PID = PID + i;
						_current_VID = VID;
						break;
					}
					await Task.Delay(1);
				}
			}
			if (connected)
			{
				PrintDebugMsg("Device: VID (0x" + _current_VID.ToString("X4") + ") PID (0x" + _current_PID.ToString("X4") + ") | MODE: " + USBMode);
				chipVersion = ChipVersion();
				PrintDebugMsg(chipVersion);
				return "Connected";
			}
			PrintDebugMsg("Failed to connect! No USB device found");
			return "Not connected";
		}

		private async Task<string> UploadAsync(string fileName = null)
		{
			string toUpdate = "";
			try
			{
				bool sPI = false;
				string previousVersion = Revision() + Build();
				PrintDebugMsg("Current Version: " + previousVersion);
				if (fileName == "")
				{
					return "Error";
				}
				progress = 0;
				PrintDebugMsg("Uploading Firmware...");
				PrintDebugMsg($"Progress: {progress}%");
				await Task.Delay(10);
				byte[] data = ReadFromFile(fileName);
				if (data == null)
				{
					PrintDebugMsg("ERROR - Could not read " + fileName);
					throw new Exception();
				}
				toUpdate = CheckVersionFromFile(data);
				if (toUpdate == previousVersion && _current_PID != 12289)
				{
					PrintDebugMsg("It's latest firmware V " + toUpdate);
					progress = 100;
					PrintDebugMsg($"Progress: {progress}%");
					WriteResults(success: true);
					return "ESS081: The firmware is updated, V " + toUpdate;
				}
				DialogResult result = MessageBox.Show("Update to version " + toUpdate + "? \n", "ESS_SuiteFW Update", MessageBoxButtons.YesNo);
				if (result != DialogResult.Yes)
				{
					return "Exit";
				}
				progress += 30;
				PrintDebugMsg($"Progress: {progress}%");
				await Task.Delay(10);
				if (GetFlashType())
				{
					sPI = true;
					byte[] read = new byte[1];
					ReadRegisterEEPROM(5u, 1, ref read);
					uint dataMajor = 0u;
					for (int i = 0; i < 1; i++)
					{
						dataMajor |= (uint)(read[i] << 8 * i);
					}
					WriteRegisterEEPROM(32768u, 1, BitConverter.GetBytes(dataMajor));
					if (!SPIErase())
					{
						throw new Exception("SPI Erase failed.");
					}
					if (!WaitSPIReady())
					{
						throw new Exception("SPI not ready.");
					}
				}
				if (!sPI && PID == 12288 && !WriteRegisterEEPROM(32760u, 1, new byte[1] { 255 }))
				{
					PrintDebugMsg("ERROR - Failed to store DRE tables");
					throw new Exception();
				}
				byte header = data[0];
				if (!WriteToMemory(data, sPI))
				{
					PrintDebugMsg("ERROR - Could not write to memory.");
					throw new Exception();
				}
				progress += 30;
				PrintDebugMsg($"Progress: {progress}%");
				PrintDebugMsg("Verifying Firmware...");
				await Task.Delay(20);
				byte[] readBack = ReadFromMemory(data.Length);
				if (readBack == null)
				{
					PrintDebugMsg("ERROR - Could not read from memory.");
					throw new Exception();
				}
				progress += 10;
				PrintDebugMsg($"Progress: {progress}%");
				await Task.Delay(10);
				if (!Compare(data, readBack))
				{
					PrintDebugMsg("ERROR - Compare failed.");
					throw new Exception();
				}
				progress += 10;
				PrintDebugMsg($"Progress: {progress}%");
				await Task.Delay(10);
				if (!WriteRegisterEEPROM(0u, 1, new byte[1] { header }))
				{
					PrintDebugMsg("ERROR - Firmware download failed");
					throw new Exception();
				}
				await Task.Delay(20);
				if (!ReadRegisterEEPROM(0u, 1, ref readBack))
				{
					PrintDebugMsg("ERROR - Firmware readback failed");
					throw new Exception();
				}
				if (header != readBack[0])
				{
					PrintDebugMsg("ERROR - Verification failed");
					throw new Exception();
				}
				progress += 20;
				PrintDebugMsg($"Progress: {progress}%");
				_ = Revision() + Build();
				PrintDebugMsg("Updated firmware V " + toUpdate + " successfully.");
				WriteResults(success: true);
				return "ESS081: Updated firmware V " + toUpdate + " successfully.";
			}
			catch (Exception ex)
			{
				Exception e = ex;
				PrintDebugMsg("Updated firmware V " + toUpdate + " Failed. Please retry.");
				WriteResults(success: false, e.Message);
				return "ESS081: Updated firmware V " + toUpdate + " Failed. Please retry.";
			}
		}

		private bool Reload()
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] array = new byte[64];
			array[0] = 49;
			array[1] = 0;
			array[3] = 1;
			if (!SendControlData(array))
			{
				return false;
			}
			return true;
		}

		public bool ReadRegister_USBBridge(byte address, ref byte data)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] array = new byte[4];
			byte[] data2 = new byte[4];
			array[0] = 29;
			array[1] = 128;
			array[2] = address;
			array[3] = 1;
			if (!SendControlData(array))
			{
				return false;
			}
			if (!ReceiveControlData(ref data2))
			{
				return false;
			}
			data = data2[0];
			return true;
		}

		private bool WriteRegister_USBBridge(byte address, byte data)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			return SendControlData(new byte[5] { 31, 128, address, 1, data });
		}

		public bool ReadRegister_DUT(byte address, ref byte data)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] data2 = new byte[4];
			if (!SendControlData(new byte[4]
			{
				4,
				DeviceAddress.SetBit(0, value: true),
				address,
				0
			}))
			{
				return false;
			}
			if (!ReceiveControlData(ref data2))
			{
				return false;
			}
			data = data2[3];
			return true;
		}

		public bool WriteRegister_DUT(byte address, byte data)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			return SendControlData(new byte[5] { 4, DeviceAddress, address, data, 0 });
		}

		private bool ReadRegister_Generic(byte address, byte data, byte devAddress)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] data2 = new byte[4];
			if (!SendControlData(new byte[4]
			{
				4,
				devAddress.SetBit(0, value: true),
				address,
				0
			}))
			{
				return false;
			}
			if (!ReceiveControlData(ref data2))
			{
				return false;
			}
			data = data2[3];
			return true;
		}

		private bool WriteRegister_Generic(byte address, byte data, byte devAddress)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			return SendControlData(new byte[5] { 4, devAddress, address, data, 0 });
		}

		private void PrintDebugMsg(string msg)
		{
			Console.WriteLine(string.Concat(DateTime.Now, ": ", msg));
		}

		private void ErrorHandler(Exception ex, bool ShowDebugMsg = true)
		{
			if (ShowDebugMsg)
			{
				PrintDebugMsg(ex.Message ?? "");
			}
			_transactionFailed = true;
		}

		private byte[] ReadFromFile(string fileName)
		{
			using BinaryReader binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
			long length = new FileInfo(fileName).Length;
			if (length > 32768)
			{
				PrintDebugMsg("ERROR - File length too large");
				return null;
			}
			return binaryReader.ReadBytes((int)length);
		}

		private byte[] ReadFromMemory(int count)
		{
			byte[] data = new byte[count];
			if (!ReadRegisterEEPROM(0u, count, ref data))
			{
				PrintDebugMsg("ERROR - Failed to read EEPROM");
				return null;
			}
			return data;
		}

		private bool WriteToMemory(byte[] data, bool SPI)
		{
			if (!WriteRegisterEEPROM(0u, data.Length, data, SPI))
			{
				PrintDebugMsg("ERROR - Failed to write EEPROM");
				return false;
			}
			return true;
		}

		private bool Compare(byte[] data1, byte[] data2)
		{
			if (data1.Length != data2.Length)
			{
				return false;
			}
			for (int i = 0; i < data1.Length; i++)
			{
				if (data1[i] != data2[i])
				{
					return false;
				}
			}
			return true;
		}

		private void WriteResults(bool success, string error = "")
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\AppData\\Results.txt";
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				using StreamWriter streamWriter = new StreamWriter(File.Create(text));
				PrintDebugMsg("Please check " + text + " for more information.");
				streamWriter.WriteLine("ESS_SuiteFW Version: " + Version);
				if (UsbDevice.Connected)
				{
					streamWriter.WriteLine("Device detected");
				}
				else
				{
					streamWriter.WriteLine("Device not detected");
				}
				streamWriter.WriteLine(chipVersion);
				streamWriter.WriteLine("Previous Version: v" + previousVersion);
				if (success)
				{
					streamWriter.WriteLine("Current Version: v" + Revision() + Build());
				}
				else
				{
					streamWriter.WriteLine("Current Version: Failed to upload.");
				}
				streamWriter.WriteLine($"Progress percentage: {progress}%");
				if (!success)
				{
					streamWriter.WriteLine("Error message: " + error);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private string CheckVersionFromFile(byte[] data)
		{
			string text = data[4].ToString();
			string text2 = data[5].ToString();
			uint num = 0u;
			for (int i = 0; i < 2; i++)
			{
				num |= (uint)(data[i + 10] << 8 * i);
			}
			return text2 + "." + text + "." + num;
		}

		private string Revision()
		{
			byte[] data = new byte[1];
			ReadRegisterEEPROM(4u, 1, ref data);
			uint num = 0u;
			for (int i = 0; i < 1; i++)
			{
				num |= (uint)(data[i] << 8 * i);
			}
			ReadRegisterEEPROM(5u, 1, ref data);
			uint num2 = 0u;
			for (int j = 0; j < 1; j++)
			{
				num2 |= (uint)(data[j] << 8 * j);
			}
			return num2 + "." + num;
		}

		private string Build()
		{
			byte[] data = new byte[2];
			ReadRegisterEEPROM(10u, 2, ref data);
			uint num = 0u;
			for (int i = 0; i < 2; i++)
			{
				num |= (uint)(data[i] << 8 * i);
			}
			return "." + num;
		}

		private string ChipVersion()
		{
			byte data = 0;
			ReadRegister(155, ref data);
			WriteRegister(155, 12, 0);
			byte data2 = 0;
			ReadRegister(155, ref data2);
			if (data2 == 12)
			{
				return "Chip Version: Production Chip";
			}
			return "Chip Version: Engineering Chip";
		}

		public bool GetFlashType(bool ShowDebugMsg = false)
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] array = new byte[64];
			byte[] data = new byte[64];
			array[0] = 41;
			array[1] = 1;
			if (!SendControlData(array, ShowDebugMsg))
			{
				return false;
			}
			if (!ReceiveControlData(ref data, ShowDebugMsg))
			{
				return false;
			}
			if (data[0] == 1)
			{
				return true;
			}
			return false;
		}

		public bool SPIErase()
		{
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				return false;
			}
			byte[] array = new byte[64];
			array[0] = 43;
			array[1] = 0;
			array[3] = 1;
			if (!SendControlData(array))
			{
				return false;
			}
			return true;
		}

		public bool WaitSPIReady()
		{
			bool result = false;
			if (UsbDevice == null || !UsbDevice.Connected)
			{
				throw new Exception("Device not detected. Please insure you have the device plugged in");
			}
			for (int i = 0; i < 1000; i++)
			{
				if (SPICheckReady())
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool SPICheckReady()
		{
			byte[] array = new byte[2];
			byte[] data = new byte[64];
			array[0] = 45;
			array[1] = 1;
			if (!SendControlData(array))
			{
				return false;
			}
			if (!ReceiveControlData(ref data))
			{
				return false;
			}
			return data[0] != 0;
		}

		public string GetFirmwareFile(string path)
		{
			string path2 = AppDomain.CurrentDomain.BaseDirectory + "\\AppData";
			string[] files = Directory.GetFiles(path2, "*.rom", SearchOption.TopDirectoryOnly);
			string[] array = files;
			foreach (string text in array)
			{
				if (text.Contains(path))
				{
					return text;
				}
			}
			return "";
		}
	}
	public class LogicCoreModel
	{
		private I2CRegisterModel _i2CRegisterModel;

		public static int GetIndexFromName(string propName)
		{
			string text = string.Empty;
			int result = -1;
			for (int i = 0; i < propName.Length; i++)
			{
				if (char.IsDigit(propName[i]))
				{
					text += propName[i];
				}
			}
			if (text.Length > 0)
			{
				result = int.Parse(text);
			}
			return result;
		}

		public static bool IsHex(IEnumerable<char> chars)
		{
			foreach (char @char in chars)
			{
				if ((@char < '0' || @char > '9') && (@char < 'a' || @char > 'f') && (@char < 'A' || @char > 'F'))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsNumeric(IEnumerable<char> chars, bool binary = false)
		{
			foreach (char @char in chars)
			{
				if (!((@char != '-') ? ((!binary) ? (@char >= '0' && @char <= '9') : (@char == '0' || @char == '1')) : (!binary)))
				{
					return false;
				}
			}
			return true;
		}

		public static string ToHex(int value)
		{
			return "0x" + value.ToString("X");
		}

		public static byte FromHex(string value)
		{
			return value.Contains("0x") ? byte.Parse(value.Substring(2), NumberStyles.HexNumber) : byte.Parse(value, NumberStyles.Integer);
		}

		public static int FromHex32(string value)
		{
			return value.Contains("0x") ? int.Parse(value.Substring(2), NumberStyles.HexNumber) : int.Parse(value, NumberStyles.Integer);
		}

		public static string ToBinary(int value)
		{
			string text = "";
			for (int num = 7; num >= 0; num--)
			{
				if ((double)value - Math.Pow(2.0, num) >= 0.0)
				{
					text += "1";
					value -= (byte)Math.Pow(2.0, num);
				}
				else
				{
					text += "0";
				}
			}
			return "0b" + text;
		}

		public static byte FromBinary(string value)
		{
			byte b = 0;
			if (value.Contains("0b"))
			{
				value = value.Substring(2);
			}
			for (int i = 0; i < 8; i++)
			{
				if (value[i] == '1')
				{
					b += (byte)Math.Pow(2.0, 7 - i);
				}
			}
			return b;
		}
	}
}
namespace ESS_SuiteFW.Interfaces
{
	public interface IUsbDevice
	{
		DigitalUsbBase UsbDevice { get; set; }
	}
}
namespace ESS_SuiteFW.Extensions
{
	public static class RegisterHelpers
	{
		public static uint SetBits(this uint b, int bit, int length, uint value)
		{
			if (bit < 0 || bit > 31 || bit + length > 32)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (value << bit) | (b & (uint)(~(-1 >>> 32 - length << bit)));
		}

		public static uint GetBits(this uint b, int bit, int length)
		{
			if (bit < 0 || bit > 31 || bit + length > 32)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (b >> bit) & (uint)(-1 >>> 32 - length);
		}

		public static bool GetBit(this uint b, int bit)
		{
			if (bit < 0 || bit > 31)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (b & (1 << bit)) == 1 << bit;
		}

		public static uint SetBit(this uint b, int bit, bool value)
		{
			if (bit < 0 || bit > 31)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			if (!value)
			{
				return b & (uint)(~(1 << bit));
			}
			return b | (uint)(1 << bit);
		}

		public static ushort SetBits(this ushort b, int bit, int length, ushort value)
		{
			if (bit < 0 || bit > 15 || bit + length > 16)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			if (value > 65535 >> 16 - length)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			return (ushort)((value << bit) | (b & ~(65535 >> 16 - length << bit)));
		}

		public static ushort GetBits(this ushort b, int bit, int length)
		{
			if (bit < 0 || bit > 16 || bit + length > 16)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (ushort)((b >> bit) & (65535 >> 16 - length));
		}

		public static bool GetBit(this ushort b, int bit)
		{
			if (bit < 0 || bit > 16)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (b & (1 << bit)) == 1 << bit;
		}

		public static ushort SetBit(this ushort b, int bit, bool value)
		{
			if (bit < 0 || bit > 16)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			if (!value)
			{
				return (ushort)(b & ~(1 << bit));
			}
			return (ushort)(b | (1 << bit));
		}

		public static byte SetBits(this byte b, int bit, int length, byte value)
		{
			if (bit < 0 || bit > 7 || bit + length > 8)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			if (value > 255 >> 8 - length)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			int num = ~(255 >> 8 - length << bit);
			return (byte)((value << bit) | (b & ~(255 >> 8 - length << bit)));
		}

		public static byte GetBits(this byte b, int bit, int length)
		{
			if (bit < 0 || bit > 7 || bit + length > 8)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (byte)((b >> bit) & (255 >> 8 - length));
		}

		public static bool GetBit(this byte b, int bit)
		{
			if (bit < 0 || bit > 7)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			return (b & (1 << bit)) == 1 << bit;
		}

		public static byte SetBit(this byte b, int bit, bool value)
		{
			if (bit < 0 || bit > 7)
			{
				throw new ArgumentOutOfRangeException("bit");
			}
			if (!value)
			{
				return (byte)(b & ~(1 << bit));
			}
			return (byte)(b | (1 << bit));
		}

		public static void InsertIntIntoByteArray(this byte[] b, uint value, int startIndex, bool littleEndian = true)
		{
			for (int i = 0; i < 4; i++)
			{
				byte b2 = (littleEndian ? ((byte)(i * 8)) : ((byte)(24 - i * 8)));
				b[startIndex + i] = (byte)((value >> (int)b2) & 0xFF);
			}
		}

		public static uint GetIntFromByteArray(this byte[] b, int startIndex, bool littleEndian = true)
		{
			uint num = 0u;
			for (int i = 0; i < 4; i++)
			{
				byte b2 = (littleEndian ? ((byte)(i * 8)) : ((byte)(24 - i * 8)));
				num = (uint)(b[startIndex + i] << (int)b2) | num;
			}
			return num;
		}
	}
}
