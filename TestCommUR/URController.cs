/*=============================================================================|
|  PROJECT URController                                                  1.0.0 |
|==============================================================================|
|  Copyright (C) 2019 Denis FRAIPONT (SICA2M)                                  |
|  All rights reserved.                                                        |
|==============================================================================|
|  URController is free class: you can redistribute it and/or modify           |
|  it under the terms of the Lesser GNU General Public License as published by |
|  the Free Software Foundation, either version 3 of the License, or           |
|  (at your option) any later version.                                         |
|                                                                              |
|  It means that you can distribute your commercial software which includes    |
|  URController without the requirement to distribute the source code          |
|  of your application and without the requirement that your application be    |
|  itself distributed under LGPL.                                              |
|                                                                              |
|  URController is distributed in the hope that it will be useful,             |
|  but WITHOUT ANY WARRANTY; without even the implied warranty of              |
|  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the               |
|  Lesser GNU General Public License for more details.                         |
|                                                                              |
|  You should have received a copy of the GNU General Public License and a     |
|  copy of Lesser GNU General Public License along with URController           |
|  If not, see  http://www.gnu.org/licenses/                                   |
|=============================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UR
{
	#region RTDE Protocol
	/// <summary>
	/// Extension to get Attribute on enum
	/// Usage: string sEnumAttributName = dtItem.GetAttribute<EnumTypeAttribute>().PropertyName;
	/// Must use same namespace
	/// </summary>
	public static class EnumExtensions
	{
		public static TAttribute GetAttribute<TAttribute>(this Enum value)
		  where TAttribute : Attribute
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			return type.GetField(name)
			            .GetCustomAttribute<TAttribute>();
		}
	}

	/// <summary>
	/// Abstract Class for all RTDE communication
	/// https://www.universal-robots.com/how-tos-and-faqs/how-to/ur-how-tos/real-time-data-exchange-rtde-guide-22229/
	/// </summary>
	internal abstract class RTDE_Package
	{
		#region HostToNetworkOrder
		/// <summary>
		/// Convert a Int32 to network order
		/// </summary>
		/// <param name="host">Int32 to convert</param>
		/// <returns>Int32 in network order</returns>
		public static byte[] HostToNetworkOrder(Int32 host)
		{
			byte[] bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}

		/// <summary>
		/// Convert a UInt32 to network order
		/// </summary>
		/// <param name="host">UInt32 to convert</param>
		/// <returns>UInt32 in network order</returns>
		public static byte[] HostToNetworkOrder(UInt32 host)
		{
			byte[] bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}

		/// <summary>
		/// Convert a UInt64 to network order
		/// </summary>
		/// <param name="host">UInt64 to convert</param>
		/// <returns>UInt64 in network order</returns>
		public static byte[] HostToNetworkOrder(UInt64 host)
		{
			byte[] bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}

		/// <summary>
		/// Convert a double to network order
		/// </summary>
		/// <param name="host">double to convert</param>
		/// <returns>double in network order</returns>
		public static byte[] HostToNetworkOrder(double host)
		{
			byte[] bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}

		/// <summary>
		/// Convert a Int32 to host order
		/// </summary>
		/// <param name="network">Int32 to convert</param>
		/// <returns>Int32 in host order</returns>
		public static Int32 NetworkToHostOrder(Int32 network)
		{
			byte[] bytes = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return BitConverter.ToInt32(bytes, 0);
		}

		/// <summary>
		/// Convert a UInt32 to host order
		/// </summary>
		/// <param name="network">UInt32 to convert</param>
		/// <returns>UInt32 in host order</returns>
		public static UInt32 NetworkToHostOrder(UInt32 network)
		{
			byte[] bytes = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return BitConverter.ToUInt32(bytes, 0);
		}

		/// <summary>
		/// Convert a UInt64 to host order
		/// </summary>
		/// <param name="network">UInt64 to convert</param>
		/// <returns>UInt64 in host order</returns>
		public static UInt64 NetworkToHostOrder(UInt64 network)
		{
			byte[] bytes = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return BitConverter.ToUInt64(bytes, 0);
		}

		/// <summary>
		/// Convert a double to host order
		/// </summary>
		/// <param name="network">double to convert</param>
		/// <returns>double in host order</returns>
		public static double NetworkToHostOrder(double network)
		{
			byte[] bytes = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return BitConverter.ToDouble(bytes, 0);
		}
		#endregion

		/// <summary>
		/// All RTDE Package type
		/// On new type, change RTDE_Package.NewPackageFrom() and URController.Read()
		/// </summary>
		public enum PackageType : byte
		{
			RTDE_REQUEST_PROTOCOL_VERSION = 86, //V
			RTDE_GET_URCONTROL_VERSION = 118, //v
			RTDE_TEXT_MESSAGE = 77, //M
			RTDE_DATA_PACKAGE = 85, //U
			RTDE_CONTROL_PACKAGE_SETUP_OUTPUTS = 79, //O
			RTDE_CONTROL_PACKAGE_SETUP_INPUTS = 73, //I
			RTDE_CONTROL_PACKAGE_START = 83, //S
			RTDE_CONTROL_PACKAGE_PAUSE = 80, //P
		}

		/// <summary>
		/// Attribute for DataType Enum.
		/// Can retrieve data with EnumExtensions
		/// Usage:
		/// RTDE_Package.DataType dtItem = ((RTDE_Package.ControllerIOData)item.Tag).Type;
		/// string sItemType = dtItem.GetAttribute<RTDE_Package.DataTypeAttribute>().Name;
		/// </summary>
		public class DataTypeAttribute : Attribute
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public int SizeInBits { get; set; }
			public int SizeInBytes { get { return SizeInBits / 8; } }
			public int SizeInBytesForString(int lenght) { return (SizeInBits / 8) * lenght; }
		}

		/// <summary>
		/// All data type
		/// </summary>
		public enum DataType
		{
			//Using DataTypeAttribute
			[DataType(Name = "BOOL",          Type = "0 = False, everything else = True", SizeInBits = 8)]      DT_BOOL,
			[DataType(Name = "UINT8",         Type = "unsigned integer",                  SizeInBits = 8)]      DT_UINT8,
			[DataType(Name = "UINT32",        Type = "unsigned integer",                  SizeInBits = 32)]     DT_UINT32,
			[DataType(Name = "UINT64",        Type = "unsigned integer",                  SizeInBits = 64)]     DT_UINT64,
			[DataType(Name = "INT32",         Type = "signed integer, two's complement",  SizeInBits = 32)]     DT_INT32,
			[DataType(Name = "DOUBLE",        Type = "IEEE 754 floating point",           SizeInBits = 64)]     DT_DOUBLE,
			[DataType(Name = "VECTOR3D",      Type = "3xDOUBLE",                          SizeInBits = 3 * 64)] DT_VECTOR3D,
			[DataType(Name = "VECTOR6D",      Type = "6xDOUBLE",                          SizeInBits = 6 * 64)] DT_VECTOR6D,
			[DataType(Name = "VECTOR6INT32",  Type = "6xINT32",                           SizeInBits = 6 * 32)] DT_VECTOR6INT32,
			[DataType(Name = "VECTOR6UINT32", Type = "6xUINT32",                          SizeInBits = 6 * 32)] DT_VECTOR6UINT32,
			[DataType(Name = "STRING",        Type = "ASCII char array",                  SizeInBits = 8)]      DT_STRING
		}

		/// <summary>
		/// IOData format for input/output exchange  Name, Type, Comment, Intro, Value
		/// </summary>
		public class ControllerIOData
		{
			public ControllerIOData(string name, RTDE_Package.DataType type, string comment, string intro)
			{
				Name = name; Type = type; Comment = comment; Intro = intro; Value = "";
			}
			public ControllerIOData(string name, RTDE_Package.DataType type, string comment, string intro, string value)
			{
				Name = name; Type = type; Comment = comment; Intro = intro; Value = value;
			}

			public string Name { get; private set; }
			public RTDE_Package.DataType Type { get; private set; }
			public string Comment { get; private set; }
			public string Intro { get; private set; }
			public string Value { get; set; }
		}

		/// <summary>
		/// List of all Input managed in RTDE
		/// These should change with new controller versions.
		/// </summary>
		public static List<ControllerIOData> ControllerInput = new List<ControllerIOData>
		{
			//                     Name                                Type                 Comment                                                                                                     Introduced in version
			new ControllerIOData( "speed_slider_mask",                 DataType.DT_UINT32 ,"0 = don't change speed slider with this input. 1 = use speed_slider_fraction to set speed slider value.",   "3.3.0/5.0.0") ,
			new ControllerIOData( "speed_slider_fraction",             DataType.DT_DOUBLE ,"new speed slider value",                                                                                    "3.3.0/5.0.0") ,
			new ControllerIOData( "standard_digital_output_mask",      DataType.DT_UINT8  ,"Standard digital output bit mask*",                                                                         "3.3.0/5.0.0") ,
			new ControllerIOData( "standard_digital_output",           DataType.DT_UINT8  ,"Standard digital outputs",                                                                                  "3.3.0/5.0.0") ,
			new ControllerIOData( "configurable_digital_output_mask",  DataType.DT_UINT8  ,"Configurable digital output bit mask*",                                                                     "3.3.0/5.0.0") ,
			new ControllerIOData( "configurable_digital_output",       DataType.DT_UINT8  ,"Configurable digital outputs",                                                                              "3.3.0/5.0.0") ,
			new ControllerIOData( "tool_digital_output_mask",          DataType.DT_UINT8  ,"Tool digital outputs mask* Bits 0-1: mask, remaining bits are reserved for future use",                     "3.3.0/5.0.0") ,
			new ControllerIOData( "tool_digital_output",               DataType.DT_UINT8  ,"Tool digital outputs Bits 0-1: output state, remaining bits are reserved for future use",                   "3.3.0/5.0.0") ,
			new ControllerIOData( "standard_analog_output_mask",       DataType.DT_UINT8  ,"Standard analog output mask Bits 0-1: standard_analog_output_0 | standard_analog_output_1",                 "3.3.0/5.0.0") ,
			new ControllerIOData( "standard_analog_output_type",       DataType.DT_UINT8  ,"Output domain {0=current[A], 1=voltage[V]} Bits 0-1: standard_analog_output_0  |  standard_analog_output_1","3.3.0/5.0.0") ,
			new ControllerIOData( "standard_analog_output_0",          DataType.DT_DOUBLE ,"Standard analog output 0 (ratio) [0..1]",                                                                   "3.3.0/5.0.0") ,
			new ControllerIOData( "standard_analog_output_1",          DataType.DT_DOUBLE ,"Standard analog output 1 (ratio) [0..1]",                                                                   "3.3.0/5.0.0") ,
			new ControllerIOData( "input_bit_registers0_to_31",        DataType.DT_UINT32 ,"General purpose bits. This range of the boolean input registers is reserved for FieldBus/PLC interface usage.",                             "3.4.0/5.0.0") ,
			new ControllerIOData( "input_bit_registers32_to_63",       DataType.DT_UINT32 ,"General purpose bits. This range of the boolean input registers is reserved for FieldBus/PLC interface usage.",                             "3.4.0/5.0.0") ,
			new ControllerIOData( "input_bit_register_64",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_65",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_66",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_67",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_68",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_69",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_70",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_71",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_72",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_73",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_74",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_75",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_76",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_77",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_78",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_79",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_80",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_81",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_82",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_83",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_84",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_85",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_86",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_87",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_88",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_89",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_90",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_91",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_92",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_93",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_94",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_95",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_96",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_97",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_98",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_99",             DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_100",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_101",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_102",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_103",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_104",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_105",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_106",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_107",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_108",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_109",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_110",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_111",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_112",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_113",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_114",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_115",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_116",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_117",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_118",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_119",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_120",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_121",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_122",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_123",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_124",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_125",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_126",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_127",            DataType.DT_BOOL   ,"64 general purpose bits. X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_0",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_1",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_2",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_3",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_4",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_5",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_6",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_7",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_8",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_9",              DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_10",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_11",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_12",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_13",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_14",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_15",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_16",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_17",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_18",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_19",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_20",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_21",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_22",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_23",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.",       "3.4.0/5.0.0") ,
			new ControllerIOData( "input_int_register_24",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_25",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_26",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_27",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_28",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_29",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_30",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_31",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_32",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_33",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_34",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_35",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_36",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_37",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_38",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_39",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_40",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_41",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_42",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_43",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_44",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_45",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_46",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_47",             DataType.DT_INT32  ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_0",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_1",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_2",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_3",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_4",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_5",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_6",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_7",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_8",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_9",           DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_10",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_11",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_12",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_13",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_14",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_15",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_16",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_17",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_18",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_19",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_20",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_21",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_22",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_23",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage. ",        "3.4.0/5.0.0") ,
			new ControllerIOData( "input_double_register_24",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_25",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_26",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_27",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_28",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_29",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_30",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_31",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_32",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_33",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_34",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_35",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_36",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_37",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_38",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_39",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_40",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_41",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_42",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_43",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_44",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_45",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_46",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_47",          DataType.DT_DOUBLE ,"48 general purpose double registers. X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0")
		};

		/// <summary>
		/// List of all Output managed in RTDE
		/// These should change with new controller versions.
		/// </summary>
		public static List<ControllerIOData> ControllerOutput = new List<ControllerIOData>
		{
			//                     Name                            Type                      Comment                                                                                                                       Introduced in version
			new ControllerIOData( "timestamp",                     DataType.DT_DOUBLE      ,"Time elapsed since the controller was started [s]",                                                                           "3.3.0/5.0.0"),
			new ControllerIOData( "target_q",                      DataType.DT_VECTOR6D    ,"Target joint positions",                                                                                                      "3.3.0/5.0.0"),
			new ControllerIOData( "target_qd",                     DataType.DT_VECTOR6D    ,"Target joint velocities",                                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "target_qdd",                    DataType.DT_VECTOR6D    ,"Target joint accelerations",                                                                                                  "3.3.0/5.0.0"),
			new ControllerIOData( "target_current",                DataType.DT_VECTOR6D    ,"Target joint currents",                                                                                                       "3.3.0/5.0.0"),
			new ControllerIOData( "target_moment",                 DataType.DT_VECTOR6D    ,"Target joint moments (torques)",                                                                                              "3.3.0/5.0.0"),
			new ControllerIOData( "actual_q",                      DataType.DT_VECTOR6D    ,"Actual joint positions",                                                                                                      "3.3.0/5.0.0"),
			new ControllerIOData( "actual_qd",                     DataType.DT_VECTOR6D    ,"Actual joint velocities",                                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "actual_current",                DataType.DT_VECTOR6D    ,"Actual joint currents",                                                                                                       "3.3.0/5.0.0"),
			new ControllerIOData( "joint_control_output",          DataType.DT_VECTOR6D    ,"Joint control currents",                                                                                                      "3.3.0/5.0.0"),
			new ControllerIOData( "actual_TCP_pose",               DataType.DT_VECTOR6D    ,"Actual Cartesian coordinates of the tool: (x,y,z,rx,ry,rz), where rx, ry and rz is a representation of the tool orientation", "3.3.0/5.0.0"),
			new ControllerIOData( "actual_TCP_speed",              DataType.DT_VECTOR6D    ,"Actual speed of the tool given in Cartesian coordinates",                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "actual_TCP_force",              DataType.DT_VECTOR6D    ,"Generalized forces in the TCP",                                                                                               "3.3.0/5.0.0"),
			new ControllerIOData( "target_TCP_pose",               DataType.DT_VECTOR6D    ,"Target Cartesian coordinates of the tool: (x,y,z,rx,ry,rz), where rx, ry and rz is a representation of the tool orientation", "3.3.0/5.0.0"),
			new ControllerIOData( "target_TCP_speed",              DataType.DT_VECTOR6D    ,"Target speed of the tool given in Cartesian coordinates",                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "actual_digital_input_bits",     DataType.DT_UINT64      ,"Current state of the digital inputs. 0-7: Standard, 8-15: Configurable, 16-17: Tool",                                         "3.3.0/5.0.0"),
			new ControllerIOData( "joint_temperatures",            DataType.DT_VECTOR6D    ,"Temperature of each joint in degrees Celsius",                                                                                "3.3.0/5.0.0"),
			new ControllerIOData( "actual_execution_time",         DataType.DT_DOUBLE      ,"Controller real-time thread execution time",                                                                                  "3.3.0/5.0.0"),
			new ControllerIOData( "robot_mode",                    DataType.DT_INT32       ,"Robot mode Please see Remote Control Via TCP/IP - 16496",                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "joint_mode",                    DataType.DT_VECTOR6INT32,"Joint control modes Please see Remote Control Via TCP/IP - 16496",                                                            "3.3.0/5.0.0"),
			new ControllerIOData( "safety_mode",                   DataType.DT_INT32       ,"Safety mode Please see Remote Control Via TCP/IP - 16496",                                                                    "3.3.0/5.0.0"),
			new ControllerIOData( "actual_tool_accelerometer",     DataType.DT_VECTOR3D    ,"Tool x, y and z accelerometer values",                                                                                        "3.3.0/5.0.0"),
			new ControllerIOData( "speed_scaling",                 DataType.DT_DOUBLE      ,"Speed scaling of the trajectory limiter",                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "target_speed_fraction",         DataType.DT_DOUBLE      ,"Target speed fraction",                                                                                                       "3.3.0/5.0.0"),
			new ControllerIOData( "actual_momentum",               DataType.DT_DOUBLE      ,"Norm of Cartesian linear momentum",                                                                                           "3.3.0/5.0.0"),
			new ControllerIOData( "actual_main_voltage",           DataType.DT_DOUBLE      ,"Safety Control Board: Main voltage",                                                                                          "3.3.0/5.0.0"),
			new ControllerIOData( "actual_robot_voltage",          DataType.DT_DOUBLE      ,"Safety Control Board: Robot voltage (48V)",                                                                                   "3.3.0/5.0.0"),
			new ControllerIOData( "actual_robot_current",          DataType.DT_DOUBLE      ,"Safety Control Board: Robot current",                                                                                         "3.3.0/5.0.0"),
			new ControllerIOData( "actual_joint_voltage",          DataType.DT_VECTOR6D    ,"Actual joint voltages",                                                                                                       "3.3.0/5.0.0"),
			new ControllerIOData( "actual_digital_output_bits",    DataType.DT_UINT64      ,"Current state of the digital outputs. 0-7: Standard, 8-15: Configurable, 16-17: Tool",                                        "3.3.0/5.0.0"),
			new ControllerIOData( "runtime_state",                 DataType.DT_UINT32      ,"Program state",                                                                                                               "3.3.0/5.0.0"),
			new ControllerIOData( "elbow_position",                DataType.DT_VECTOR3D    ,"Actual cartesian coordinates of the elbow (x,y,z)",                                                                           "3.3.0/5.0.0"),
			new ControllerIOData( "elbow_velocity",                DataType.DT_VECTOR3D    ,"Actual cartesian velocity of the elbow",                                                                                      "3.3.0/5.0.0"),
			new ControllerIOData( "robot_status_bits",             DataType.DT_UINT32      ,"Bits 0-3: Is power on | Is program running | Is teach button pressed | Is power button pressed",                              "3.3.0/5.0.0"),
			new ControllerIOData( "safety_status_bits",            DataType.DT_UINT32      ,"Bits 0-10: Is normal mode | Is reduced mode | | Is protective stopped | Is recovery mode | Is s Is system emergencsopped | Is robot emergency stopped | Is emergency stopped | Is violatiostopped due to safety","3.3.0/5.0.0"),
			new ControllerIOData( "analog_io_types",               DataType.DT_UINT32      ,"Bits 0-3: analog input 0 | analog input 1 | analog output 0 | analog output 1, {0=current[A], 1",                             "3.3.0/5.0.0"),
			new ControllerIOData( "standard_analog_input0",        DataType.DT_DOUBLE      ,"Standard analog input 0 [A or V]",                                                                                            "3.3.0/5.0.0"),
			new ControllerIOData( "standard_analog_input1",        DataType.DT_DOUBLE      ,"Standard analog input 1 [A or V]",                                                                                            "3.3.0/5.0.0"),
			new ControllerIOData( "standard_analog_output0",       DataType.DT_DOUBLE      ,"Standard analog output 0 [A or V]",                                                                                           "3.3.0/5.0.0"),
			new ControllerIOData( "standard_analog_output1",       DataType.DT_DOUBLE      ,"Standard analog output 1 [A or V]",                                                                                           "3.3.0/5.0.0"),
			new ControllerIOData( "io_current",                    DataType.DT_DOUBLE      ,"I/O current [A]",                                                                                                             "3.3.0/5.0.0"),
			new ControllerIOData( "euromap67_input_bits",          DataType.DT_UINT32      ,"Euromap67 input bits",                                                                                                        "3.3.0/5.0.0"),
			new ControllerIOData( "euromap67_output_bits",         DataType.DT_UINT32      ,"Euromap67 output bits",                                                                                                       "3.3.0/5.0.0"),
			new ControllerIOData( "euromap67_24V_voltage",         DataType.DT_DOUBLE      ,"Euromap 24V voltage [V]",                                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "euromap67_24V_current",         DataType.DT_DOUBLE      ,"Euromap 24V current [A]",                                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "tool_mode",                     DataType.DT_UINT32      ,"Tool mode Please see Remote Control Via TCP/IP - 16496",                                                                      "3.3.0/5.0.0"),
			new ControllerIOData( "tool_analog_input_types",       DataType.DT_UINT32      ,"Output domain {0=current[A], 1=voltage[V]} Bits 0-1: tool_analog_input_0 | tool_analog_input_1",                              "3.3.0/5.0.0"),
			new ControllerIOData( "tool_analog_input0",            DataType.DT_DOUBLE      ,"Tool analog input 0 [A or V]",                                                                                                "3.3.0/5.0.0"),
			new ControllerIOData( "tool_analog_input1",            DataType.DT_DOUBLE      ,"Tool analog input 1 [A or V]",                                                                                                "3.3.0/5.0.0"),
			new ControllerIOData( "tool_output_voltage",           DataType.DT_INT32       ,"Tool output voltage [V]",                                                                                                     "3.3.0/5.0.0"),
			new ControllerIOData( "tool_output_current",           DataType.DT_DOUBLE      ,"Tool current [A]",                                                                                                            "3.3.0/5.0.0"),
			new ControllerIOData( "tool_temperature",              DataType.DT_DOUBLE      ,"Tool temperature in degrees Celsius",                                                                                         "3.3.0/5.0.0"),
			new ControllerIOData( "tcp_force_scalar",              DataType.DT_DOUBLE      ,"TCP force scalar [N]",                                                                                                        "3.3.0/5.0.0"),
			new ControllerIOData( "output_bit_registers0_to_31",   DataType.DT_UINT32      ,"General purpose bits",                                                                                                        "3.3.0/5.0.0"),
			new ControllerIOData( "output_bit_registers32_to_63",  DataType.DT_UINT32      ,"General purpose bits",                                                                                                        "3.3.0/5.0.0"),
			new ControllerIOData( "output_bit_register_64",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_65",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_66",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_67",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_68",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_69",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_70",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_71",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_72",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_73",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_74",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_75",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_76",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_77",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_78",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_79",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_80",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_81",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_82",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_83",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_84",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_85",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_86",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_87",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_88",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_89",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_90",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_91",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_92",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_93",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_94",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_95",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_96",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_97",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_98",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_99",        DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_100",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_101",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_102",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_103",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_104",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_105",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_106",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_107",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_108",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_109",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_110",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_111",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_112",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_113",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_114",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_115",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_116",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_117",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_118",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_119",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_120",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_121",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_122",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_123",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_124",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_125",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_126",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_bit_register_127",       DataType.DT_UINT32      ,"64 general purpose bits. X: [64..127] - The upper range of the boolean output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_0",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_1",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_2",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_3",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_4",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_5",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_6",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_7",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_8",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_9",         DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_10",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_11",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_12",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_13",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_14",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_15",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_16",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_17",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_18",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_19",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_20",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_21",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_22",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_23",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [0..23] - The lower range of the integer output registers is reserved for FieldBus/PLC interface usage.",       "3.3.0/5.0.0"),
			new ControllerIOData( "output_int_register_24",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_25",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_26",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_27",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_28",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_29",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_30",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_31",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_32",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_33",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_34",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_35",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_36",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_37",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_38",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_39",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_40",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_41",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_42",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_43",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_44",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_45",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_46",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_int_register_47",        DataType.DT_INT32       ,"48 general purpose integer registers. X: [24..47] - The upper range of the integer output registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_0",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_1",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_2",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_3",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_4",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_5",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_6",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_7",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_8",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_9",      DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_10",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_11",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_12",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_13",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_14",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_15",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_16",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_17",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_18",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_19",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_20",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_21",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_22",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_23",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [0..23] - The lower range of the double output registers is reserved for FieldBus/PLC interface usage.",         "3.3.0/5.0.0"),
			new ControllerIOData( "output_double_register_24",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_25",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_26",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_27",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_28",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_29",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_30",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_31",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_32",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_33",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_34",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_35",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_36",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_37",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_38",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_39",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_40",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_41",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_42",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_43",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_44",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_45",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_46",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "output_double_register_47",     DataType.DT_DOUBLE      ,"48 general purpose double registers. X: [24..47] - The upper range of the double output registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0"),
			new ControllerIOData( "input_bit_registers0_to_31",    DataType.DT_UINT32      ,"General purpose bits (input read back) This range of the boolean input registers is reserved for FieldBus/PLC interface usage.",                                                                                      "3.4.0/5.0.0"),
			new ControllerIOData( "input_bit_registers32_to_63",   DataType.DT_UINT32      ,"General purpose bits (input read back) This range of the boolean input registers is reserved for FieldBus/PLC interface usage.",                                                                                      "3.4.0/5.0.0"),
			new ControllerIOData( "input_bit_register_64",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_65",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_66",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_67",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_68",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_69",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_70",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_71",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_72",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_73",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_74",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_75",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_76",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_77",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_78",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_79",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_80",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_81",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_82",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_83",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_84",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_85",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_86",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_87",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_88",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_89",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_90",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_91",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_92",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_93",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_94",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_95",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_96",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_97",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_98",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_99",         DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_100",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_101",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_102",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_103",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_104",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_105",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_106",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_107",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_108",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_109",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_110",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_111",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_112",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_113",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_114",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_115",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_116",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_117",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_118",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_119",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_120",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_121",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_122",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_123",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_124",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_125",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_126",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_bit_register_127",        DataType.DT_BOOL        ,"64 general purpose bits (input read back) X: [64..127] - The upper range of the boolean input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_0",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_1",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_2",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_3",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_4",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_5",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_6",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_7",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_8",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_9",          DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_10",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_11",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_12",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_13",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_14",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_15",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_16",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_17",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_18",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_19",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_20",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_21",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_22",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_23",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [0..23] - The lower range of the integer input registers is reserved for FieldBus/PLC interface usage.", "3.4.0/5.0.0"),
			new ControllerIOData( "input_int_register_24",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_25",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_26",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_27",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_28",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_29",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_30",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_31",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_32",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_33",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_34",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_35",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_36",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_37",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_38",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_39",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_40",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_41",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_42",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_43",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_44",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_45",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_46",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_int_register_47",         DataType.DT_INT32       ,"48 general purpose integer registers (input read back) X: [24..47] - The upper range of the integer input registers can be used by external RTDE clients (i.e URCAPS).", "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_0",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_1",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_2",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_3",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_4",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_5",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_6",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_7",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_8",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_9",       DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_10",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_11",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_12",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_13",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_14",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_15",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_16",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_17",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_18",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_19",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_20",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_21",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_22",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_23",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [0..23] - The lower range of the double input registers is reserved for FieldBus/PLC interface usage.",         "3.4.0/5.0.0"),
			new ControllerIOData( "input_double_register_24",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_25",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_26",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_27",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_28",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_29",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_30",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_31",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_32",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_33",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_34",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_35",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_36",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_37",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_38",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_39",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_40",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_41",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_42",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_43",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_44",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_45",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_46",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "input_double_register_47",      DataType.DT_DOUBLE      ,"48 general purpose double registers (input read back) X: [24..47] - The upper range of the double input registers can be used by external RTDE clients (i.e URCAPS).",   "3.9.0/5.3.0") ,
			new ControllerIOData( "tool_output_mode",              DataType.DT_UINT8       ,"The current output mode",                                                                                                     "5.2.0"),
			new ControllerIOData( "tool_digital_output0_mode",     DataType.DT_UINT8       ,"The current mode of digital output 0",                                                                                        "5.2.0"),
			new ControllerIOData( "tool_digital_output1_mode",     DataType.DT_UINT8       ,"The current mode of digital output 1",                                                                                        "5.2.0"),
		};

		/// <summary>
		/// Default Constructors
		/// </summary>
		/// <param name="_type"></param>
		protected RTDE_Package(PackageType _type) { Type = _type; }
		protected RTDE_Package(PackageType _type, ushort _protoVers) { Type = _type; ProtoVersion = _protoVers; }

		//Header
		public PackageType Type { get; private set; } = 0;
		public ushort Size { get; private set; } = 0;

		public ushort ProtoVersion { get; private set; } = 1;

		/// <summary>
		/// Send data to controller
		/// Common method for all derived class
		/// </summary>
		/// <param name="ns">The stream where to send data.</param>
		public void SendTo(NetworkStream ns)
		{
			byte[] toSend = ToByte();
			ns.Write(toSend, 0, toSend.Length);
		}

		/// <summary>
		/// Send data to controller
		/// Common method for all derived class
		/// </summary>
		/// <returns>byte array of data to send.</returns>
		public byte[] ToByte()
		{
			List<byte> output = new List<byte>();
			byte[] payload = GetPayload();
			output.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(payload.Length + 3))));
			output.Add((byte)Type);
			output.AddRange(payload);
			return output.ToArray();
		}
		protected abstract byte[] GetPayload();

		/// <summary>
		/// Define a new package depending of what was received.
		/// </summary>
		/// <param name="ns">Stream where to get data.</param>
		/// <returns></returns>
		public static RTDE_Package NewPackageFrom(NetworkStream ns)
		{
			RTDE_Package newPackage = null;
			byte[] sizeBytes = new byte[2];
			Debug.Print( "NewPackageFrom reading ns " + ns.ToString());
			try
			{
				if (ns.Read(sizeBytes, 0, sizeBytes.Length) == 2)
				{
					int size = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(sizeBytes, 0));
					Debug.Print( "NewPackageFrom Received a packet of size=" + size.ToString());
					int typeByte = ns.ReadByte();
					if (typeByte > 0)
					{
						PackageType type = (PackageType)typeByte;
						Debug.Print( "NewPackageFrom Received a packet of type=" + type.ToString());
						switch (typeByte)
						{
							case (int)PackageType.RTDE_REQUEST_PROTOCOL_VERSION:      newPackage = new RTDE_Request_Protocol_Version(2);     break;
							case (int)PackageType.RTDE_GET_URCONTROL_VERSION:         newPackage = new RTDE_Get_URControl_Version();         break;
							case (int)PackageType.RTDE_TEXT_MESSAGE:                  newPackage = new RTDE_Text_Message();                  break;
							case (int)PackageType.RTDE_CONTROL_PACKAGE_SETUP_OUTPUTS: newPackage = new RTDE_Control_Package_Setup_Outputs(); break;
							case (int)PackageType.RTDE_CONTROL_PACKAGE_SETUP_INPUTS:  newPackage = new RTDE_Control_Package_Setup_Inputs();  break;
							case (int)PackageType.RTDE_DATA_PACKAGE:                  newPackage = new RTDE_Data_Package();                  break;
							case (int)PackageType.RTDE_CONTROL_PACKAGE_START:         newPackage = new RTDE_Control_Package_Start();         break;
							case (int)PackageType.RTDE_CONTROL_PACKAGE_PAUSE:         newPackage = new RTDE_Control_Package_Pause();         break;
							default: break;
						}
						if (newPackage != null)
						{
							Debug.Print( "NewPackageFrom Reading package.");
							byte[] bytes = new byte[size-3];
							int bytesRead = ns.Read(bytes, 0, bytes.Length);
							if (bytesRead == (size - 3))
							{
								int offset = 0;
								if (!newPackage.PayloadParser(bytes, ref offset))
									newPackage = null;
							}
						}

					}
				} else {
					Debug.Print("NewPackageFrom Can't read 2 first bytes." );
				}

			}
			catch (Exception e)
			{
				Debug.Print(e.ToString());
				throw;
			}
			return newPackage;
		}

		/// <summary>
		/// Parse data received.
		/// </summary>
		/// <param name="stream">byte array of data read.</param>
		/// <returns></returns>
		public bool Parser(byte[] stream)
		{
			//Header always start at 0
			int offset = 0;
			if (stream.Length >= 3)
			{
				Size = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(stream, 0));
				offset += sizeof(ushort);
				Type = (PackageType)stream[2];
				offset += sizeof(PackageType);

				return PayloadParser(stream, ref offset);
			}
			return false;
		}
		protected abstract bool PayloadParser(byte[] stream, ref int offset);

	}

	class RTDE_Request_Protocol_Version : RTDE_Package
	{
		//public RTDE_ReqProtoVer() : base(PackageType.RTDE_REQUEST_PROTOCOL_VERSION) { }
		public RTDE_Request_Protocol_Version(UInt16 V) : base(PackageType.RTDE_REQUEST_PROTOCOL_VERSION) { version = V; }

		private UInt16 version;
		public UInt16 Version { get { return version; } set { if ((1 <= value) && (value <= 2)) version = value; } }
		public bool Accepted { get; private set; }

		protected override byte[] GetPayload()
		{
			return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)version));
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length >= offset + 1)
			{
				Accepted = (stream[offset] != 0);
				++offset;
				return true;
			}
			return false;
		}

	}

	class RTDE_Get_URControl_Version : RTDE_Package
	{
		public RTDE_Get_URControl_Version() : base(PackageType.RTDE_GET_URCONTROL_VERSION) { }
		public UInt32 Major { get; private set; }
		public UInt32 Minor { get; private set; }
		public UInt32 Bugfix { get; private set; }
		public UInt32 Build { get; private set; }

		protected override byte[] GetPayload()
		{
			return new byte[0];
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length > offset + 1)
			{
				Major = (UInt32)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(stream, offset));
				offset += sizeof(UInt32);
				Minor = (UInt32)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(stream, offset));
				offset += sizeof(UInt32);
				Bugfix = (UInt32)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(stream, offset));
				offset += sizeof(UInt32);
				Build = (UInt32)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(stream, offset));
				offset += sizeof(UInt32);
				return true;
			}
			return false;
		}

	}

	class RTDE_Text_Message : RTDE_Package
	{
		public RTDE_Text_Message() : base(PackageType.RTDE_TEXT_MESSAGE) { }
		public byte MessageLen { get { return (byte)Message.Length; } }
		private string _message;
		public string Message { get { return _message; } set { if (value.Length > 255) { _message = value.Substring(1, 255); } else { _message = value; } } }
		public byte SourceLen { get { return (byte)Source.Length; } }
		private string _source;
		public string Source { get { return _source; } set { if (value.Length > 255) { _source = value.Substring(1, 255); } else { _source = value; } } }
		public byte WarningLevel { get; set; }

		protected override byte[] GetPayload()
		{
			int offset = 0;
			byte[] payload = new byte[MessageLen + SourceLen + 3];
			Buffer.BlockCopy(BitConverter.GetBytes(MessageLen), 0, payload, 0, 1);               offset = 1;
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(Message), 0, payload, offset, MessageLen);  offset += MessageLen;
			Buffer.BlockCopy(BitConverter.GetBytes(SourceLen), 0, payload, offset, 1);           offset += 1;
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(Source), 0, payload, offset, SourceLen);    offset += SourceLen;
			Buffer.BlockCopy(BitConverter.GetBytes(WarningLevel), 0, payload, offset, 1);
			return payload;
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length > offset + 1)
			{
				byte messLen = stream[offset];
				offset += sizeof(byte);
				if (stream.Length > (messLen + offset + 2))
				{
					Message = Encoding.ASCII.GetString(stream, offset, messLen);
					byte srcLen = stream[offset];
					offset += sizeof(byte);
					if (stream.Length > (srcLen + offset + 1))
					{
						Source = Encoding.ASCII.GetString(stream, offset, srcLen);
						WarningLevel = stream[offset];
						offset += sizeof(byte);
						return true;
					}
					else
					{
						Console.WriteLine("Received RTDE_Text_Message with wrong source lenght. Read it as V1.");
						Source = "V1";
						WarningLevel = messLen;
						Message = Encoding.ASCII.GetString(stream, 1, stream.Length-1);
						offset = stream.Length;
						return true;
					}
				}
				else
				{ Console.WriteLine("Received RTDE_Text_Message with wrong message lenght."); }
			}
			return false;
		}

	}

	class RTDE_Control_Package_Setup_Outputs : RTDE_Package
	{
		public RTDE_Control_Package_Setup_Outputs() : base(PackageType.RTDE_CONTROL_PACKAGE_SETUP_OUTPUTS) { }
		public RTDE_Control_Package_Setup_Outputs(ushort protoVers) : base(PackageType.RTDE_CONTROL_PACKAGE_SETUP_OUTPUTS, protoVers) {  }
		public double OutputFreq { get; set; }
		private string _var_names;
		public string Var_Names { get { return _var_names; } set { if (value.Length > 255) { _var_names = value.Substring(1, 255); } else { _var_names = value; } } }
		private string _var_types;
		public string Var_Types { get { return _var_types; } private set { if (value.Length > 255) { _var_types = value.Substring(1, 255); } else { _var_types = value; } } }
		public byte RecipeID { get; private set; }

		protected override byte[] GetPayload()
		{
			int offset = 0;
			if (ProtoVersion==2)
			{
				byte[] payload = new byte[Var_Names.Length + sizeof(double)];
				Buffer.BlockCopy(HostToNetworkOrder(OutputFreq), 0, payload, offset, sizeof(double));        offset += sizeof(double);
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(Var_Names), 0, payload, offset, Var_Names.Length);  offset += Var_Names.Length;
				return payload;
			}
			else
			{
				//Use Proto version 1 by default
				byte[] payload = new byte[Var_Names.Length];
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(Var_Names), 0, payload, offset, Var_Names.Length);  offset += Var_Names.Length;
				return payload;
			}
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length >= offset + 1)
			{
				if (stream[offset] == 1 || stream[offset] ==0)
				{
					//Proto Version 2
					RecipeID = stream[offset];                                                   offset += sizeof(byte);
					Var_Types = Encoding.ASCII.GetString(stream, offset, stream.Length- offset); offset += Var_Types.Length;
					return true;
				}
				else
				{
					//Proto Version 1
					RecipeID = 0;
					Var_Types = Encoding.ASCII.GetString(stream, offset, stream.Length - offset); offset += Var_Types.Length;
					return true;
				}
			}
			return false;
		}

	}

	class RTDE_Control_Package_Setup_Inputs : RTDE_Package
	{
		public RTDE_Control_Package_Setup_Inputs() : base(PackageType.RTDE_CONTROL_PACKAGE_SETUP_INPUTS) { }
		private string _var_names;
		public string Var_Names { get { return _var_names; } set { if (value.Length > 255) { _var_names = value.Substring(1, 255); } else { _var_names = value; } } }
		private string _var_types;
		public string Var_Types { get { return _var_types; } private set { if (value.Length > 255) { _var_types = value.Substring(1, 255); } else { _var_types = value; } } }
		public byte RecipeID { get; private set; }

		protected override byte[] GetPayload()
		{
			int offset = 0;
			byte[] payload = new byte[Var_Names.Length];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(Var_Names), 0, payload, offset, Var_Names.Length); offset += Var_Names.Length;
			return payload;
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length >= offset + 1)
			{
				RecipeID = stream[offset]; offset += sizeof(byte);
				Var_Types = Encoding.ASCII.GetString(stream, offset, stream.Length - offset); offset += Var_Types.Length;
				return true;
			}
			return false;
		}

	}

	class RTDE_Data_Package : RTDE_Package
	{
		public RTDE_Data_Package() : base(PackageType.RTDE_DATA_PACKAGE) { }
		public RTDE_Data_Package(byte recipeID) : base(PackageType.RTDE_DATA_PACKAGE) { RecipeID = recipeID; }
		private byte[] _datas;
		public byte RecipeID { get; private set; }

		protected override byte[] GetPayload()
		{
			int offset = 0;
			byte[] payload = new byte[_datas.Length + 1];
			payload[0] = RecipeID;                                       offset += 1;
			Buffer.BlockCopy(_datas, 0, payload, offset, _datas.Length); offset += _datas.Length;
			return payload;
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length > offset + 1)
			{
				RecipeID = stream[offset];                                           offset += sizeof(byte);
				_datas = new byte[stream.Length - offset];
				Buffer.BlockCopy(stream, offset, _datas, 0, stream.Length - offset); offset += _datas.Length;
				return true;
			}
			return false;
		}

		public static bool IsValueValid(DataType type, ref string value)
		{
			return IsValueValid(type, ref value, out byte[] formatted);
		}
		public static bool IsValueValid(DataType type, ref string value, out byte[] formatted)
		{
			if (value?.Length > 0)
			{
				switch (type)
				{
					case DataType.DT_BOOL:
						{
							bool bValue = !(value.ToLower().Contains("true") || value == "0");
							value = bValue.ToString();
							formatted = new byte[1];
							formatted[0] = bValue ? (byte)1 : (byte)0;
							return true;
						}
					case DataType.DT_UINT8:
						{
							if (byte.TryParse(value, out byte bytValue))
							{
								value = bytValue.ToString();
								formatted = new byte[1];
								formatted[0] = bytValue;
								return true;
							}
						}
						break;
					case DataType.DT_UINT32:
						{
							if (UInt32.TryParse(value, out UInt32 uint32Value))
							{
								value = uint32Value.ToString();
								formatted = HostToNetworkOrder(uint32Value);
								return true;
							}
						}
						break;
					case DataType.DT_UINT64:
						{
							if (UInt64.TryParse(value, out UInt64 uint64Value))
							{
								value = uint64Value.ToString();
								formatted = HostToNetworkOrder(uint64Value);
								return true;
							}
						}
						break;
					case DataType.DT_INT32:
						{
							if (Int32.TryParse(value, out Int32 int32Value))
							{
								value = int32Value.ToString();
								formatted = BitConverter.GetBytes(int32Value);
								return true;
							}
						}
						break;
					case DataType.DT_DOUBLE:
						{
							if (double.TryParse(value, out double doubleValue))
							{
								value = doubleValue.ToString();
								formatted = HostToNetworkOrder(doubleValue);
								return true;
							}
						}
						break;
					case DataType.DT_VECTOR3D:
						{
							string[] values = value.Split(';');
							if (values.Length == 3)
							{
								bool bOK = true;
								double[] doubleValues = new double[3];
								int curs = 0;
								foreach (string uniValue in values)
								{
									if (double.TryParse(value, out doubleValues[curs]))
									{
										value = doubleValues[curs].ToString() + ";";
									}
									else
									{
										bOK = false;
										break;
									}
								}
								if (bOK)
								{
									value = value.TrimEnd(';');
									int offset = 0;
									formatted = new byte[3 * type.GetAttribute<RTDE_Package.DataTypeAttribute>().SizeInBytes];
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[0]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[1]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[2]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									return true;
								}
							}
						}
						break;
					case DataType.DT_VECTOR6D:
						{
							string[] values = value.Split(';');
							if (values.Length == 6)
							{
								bool bOK = true;
								double[] doubleValues = new double[6];
								int curs = 0;
								foreach (string uniValue in values)
								{
									if (double.TryParse(value, out doubleValues[curs]))
									{
										value = doubleValues[curs].ToString() + ";";
									}
									else
									{
										bOK = false;
										break;
									}
								}
								if (bOK)
								{
									value = value.TrimEnd(';');
									int offset = 0;
									formatted = new byte[6 * type.GetAttribute<RTDE_Package.DataTypeAttribute>().SizeInBytes];
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[0]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[1]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[2]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[3]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[4]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									Buffer.BlockCopy(HostToNetworkOrder(doubleValues[5]), 0, formatted, offset, sizeof(double)); offset += sizeof(double);
									return true;
								}
							}
						}
						break;
					case DataType.DT_VECTOR6INT32:
						{
							string[] values = value.Split(';');
							if (values.Length == 6)
							{
								bool bOK = true;
								Int32[] int32Values = new Int32[6];
								int curs = 0;
								foreach (string uniValue in values)
								{
									if (Int32.TryParse(value, out int32Values[curs]))
									{
										value = int32Values[curs].ToString() + ";";
									}
									else
									{
										bOK = false;
										break;
									}
								}
								if (bOK)
								{
									value = value.TrimEnd(';');
									int offset = 0;
									formatted = new byte[6 * type.GetAttribute<RTDE_Package.DataTypeAttribute>().SizeInBytes];
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[0]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[1]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[2]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[3]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[4]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									Buffer.BlockCopy(HostToNetworkOrder(int32Values[5]), 0, formatted, offset, sizeof(Int32)); offset += sizeof(Int32);
									return true;
								}
							}
						}
						break;
					case DataType.DT_VECTOR6UINT32:
						{
							string[] values = value.Split(';');
							if (values.Length == 6)
							{
								bool bOK = true;
								UInt32[] uint32Values = new UInt32[6];
								int curs = 0;
								foreach (string uniValue in values)
								{
									if (UInt32.TryParse(value, out uint32Values[curs]))
									{
										value = uint32Values[curs].ToString() + ";";
									}
									else
									{
										bOK = false;
										break;
									}
								}
								if (bOK)
								{
									value = value.TrimEnd(';');
									int offset = 0;
									formatted = new byte[6 * type.GetAttribute<RTDE_Package.DataTypeAttribute>().SizeInBytes];
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[0]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[1]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[2]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[3]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[4]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									Buffer.BlockCopy(HostToNetworkOrder(uint32Values[5]), 0, formatted, offset, sizeof(UInt32)); offset += sizeof(UInt32);
									return true;
								}
							}
						}
						break;
					case DataType.DT_STRING:
						throw new NotImplementedException();
					default:
						break;
				}
			}
			formatted = new byte[0];
			return false;
		}

		public bool GetData(ref List<RTDE_Package.ControllerIOData> list)
		{
			int offset = 0;
			foreach (RTDE_Package.ControllerIOData dtItem in list)
			{
				if (offset > _datas.Length) return false;

				switch (dtItem.Type)
				{
					case DataType.DT_BOOL:
						dtItem.Value = BitConverter.ToBoolean(_datas, offset).ToString();
						break;
					case DataType.DT_UINT8:
						dtItem.Value = _datas[offset].ToString();
						break;
					case DataType.DT_UINT32:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset)).ToString();
						break;
					case DataType.DT_UINT64:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToUInt64(_datas, offset)).ToString();
						break;
					case DataType.DT_INT32:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToInt32(_datas, offset)).ToString();
						break;
					case DataType.DT_DOUBLE:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToDouble(_datas, offset)).ToString();
						break;
					case DataType.DT_VECTOR3D:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToDouble(_datas, offset)).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 1 * sizeof(double))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 2 * sizeof(double))).ToString();
						break;
					case DataType.DT_VECTOR6D:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToDouble(_datas, offset)).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 1 * sizeof(double))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 2 * sizeof(double))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 3 * sizeof(double))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 4 * sizeof(double))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToDouble(_datas, offset + 5 * sizeof(double))).ToString();
						break;
					case DataType.DT_VECTOR6INT32:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToInt32(_datas, offset)).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToInt32(_datas, offset + 1 * sizeof(Int32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToInt32(_datas, offset + 2 * sizeof(Int32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToInt32(_datas, offset + 3 * sizeof(Int32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToInt32(_datas, offset + 4 * sizeof(Int32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToInt32(_datas, offset + 5 * sizeof(Int32))).ToString();
						break;
					case DataType.DT_VECTOR6UINT32:
						dtItem.Value = NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset)).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset + 1 * sizeof(UInt32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset + 2 * sizeof(UInt32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset + 3 * sizeof(UInt32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset + 4 * sizeof(UInt32))).ToString() + ";";
						dtItem.Value += NetworkToHostOrder(BitConverter.ToUInt32(_datas, offset + 5 * sizeof(UInt32))).ToString();
						break;
					case DataType.DT_STRING:
						break;
					default:
						break;
				}
				offset += dtItem.Type.GetAttribute<RTDE_Package.DataTypeAttribute>().SizeInBytes;
			}
			return (offset == _datas.Length);
		}
		public bool SetData(List<RTDE_Package.ControllerIOData> list)
		{
			bool bOK = true;
			int lenght = 0;
			List<byte[]> bytDatas = new List<byte[]>();
			foreach(RTDE_Package.ControllerIOData ctrlIOData in list)
			{
				string strValue = ctrlIOData.Value;
				if (IsValueValid(ctrlIOData.Type, ref strValue, out byte[] bytData))
				{
					bytDatas.Add(bytData);
					lenght += bytData.Length;
				}
				else
					bOK = false;
			}
			if (bOK)
			{
				_datas = new byte[lenght];
				int offset = 0;
				foreach ( byte[] bytData in bytDatas)
				{
					Buffer.BlockCopy(bytData, 0, _datas, offset, bytData.Length); offset += bytData.Length;
				}
			}
			return bOK;
		}
	}

	class RTDE_Control_Package_Start : RTDE_Package
	{
		public RTDE_Control_Package_Start() : base(PackageType.RTDE_CONTROL_PACKAGE_START) {}

		public bool Accepted { get; private set; }

		protected override byte[] GetPayload()
		{
			return new byte[0];
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length >= offset + 1)
			{
				Accepted = (stream[offset] != 0);
				++offset;
				return true;
			}
			return false;
		}

	}

	class RTDE_Control_Package_Pause : RTDE_Package
	{
		public RTDE_Control_Package_Pause() : base(PackageType.RTDE_CONTROL_PACKAGE_PAUSE) { }

		public bool Accepted { get; private set; }

		protected override byte[] GetPayload()
		{
			return new byte[0];
		}

		protected override bool PayloadParser(byte[] stream, ref int offset)
		{
			if (stream.Length >= offset + 1)
			{
				Accepted = (stream[offset] != 0);
				++offset;
				return true;
			}
			return false;
		}

	}
	#endregion

	/// <summary>
	/// Extensions for TcpClient
	/// </summary>
	public static class TcpClientExtensions
	{
		public static TcpState GetState(this TcpClient tcpClient)
		{
			var foo = IPGlobalProperties.GetIPGlobalProperties()
				.GetActiveTcpConnections()
				.SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint)
													 && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint)
				);

			return foo != null ? foo.State : TcpState.Unknown;
		}
	}

	/*****************************************************************************/
	/// <summary>
	/// Main Class
	/// </summary>
	class URController
	{
		~URController() { Disconnect(); }
		private static Mutex mut = new Mutex();
		private bool stopReading;

		private string ip;
		public string IP
		{
			get { return ip; }
			set
			{
				try
				{
					Disconnect();
					IPAddress address = IPAddress.Parse(value);
					ip = address.ToString();
				}
				catch (Exception) { }

			}
		}
		private TcpClient client;
		private NetworkStream ns;

		public event EventHandler RecProtoVersionEvent;
		public int ProtoVersion { get; private set; }
		public event EventHandler RecURCtrlVersionEvent;
		public UInt32 URCtrlV_Major { get; private set; }
		public UInt32 URCtrlV_Minor { get; private set; }
		public UInt32 URCtrlV_Bugfix { get; private set; }
		public UInt32 URCtrlV_Build { get; private set; }
		public string URCtrlV { get { return URCtrlV_Major.ToString() + "." + URCtrlV_Minor.ToString() + "." + URCtrlV_Bugfix.ToString() + "." + URCtrlV_Build.ToString(); } }
		public event EventHandler RecMessageEvent;
		public string LastMessage { get; private set; }
		public string LastMessageSource { get; private set; }
		public event EventHandler RecCtrlPackSetupOut;
		public byte LastOutSetupRecipeID;
		public string LastOutSetupVarTypes { get; private set; }
		public event EventHandler RecCtrlPackSetupIn;
		public byte LastInSetupRecipeID;
		public string LastInSetupVarTypes { get; private set; }
		public event EventHandler RecDataPackage;
		public List<RTDE_Package.ControllerIOData> LastRequestedOutputs = new List<RTDE_Package.ControllerIOData>();
		public List<RTDE_Package.ControllerIOData> LastValidatedOutputs = new List<RTDE_Package.ControllerIOData>();
		public List<RTDE_Package.ControllerIOData> LastRequestedInputs = new List<RTDE_Package.ControllerIOData>();
		public List<RTDE_Package.ControllerIOData> LastValidatedInputs = new List<RTDE_Package.ControllerIOData>();
		public event EventHandler RecCtrlPackStartDone;
		public bool CtrlPackageRunning = false;

		/// <summary>
		/// To test if controller is Online
		/// </summary>
		/// <returns>True if Online</returns>
		public bool TestOnline()
		{
			if (IP == "") return false;
			Ping pingSender = new Ping();
			PingOptions options = new PingOptions
			{
				// Use the default Ttl value which is 128, but change the fragmentation behavior.
				DontFragment = true
			};

			// Create a buffer of 32 bytes of data to be transmitted.
			string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
			byte[] buffer = Encoding.ASCII.GetBytes(data);
			int timeout = 120;
			PingReply reply = pingSender.Send(IP, timeout, buffer, options);

			return reply.Status == IPStatus.Success;
		}

		/// <summary>
		/// Test if TCPClient is made and connected 
		/// </summary>
		/// <returns>True if connected</returns>
		public bool IsConnected()
		{
			if (client != null)
				return client.GetState() == TcpState.Established;
			return false;
		}

		public event EventHandler OnConnected;
		/// <summary>
		/// Connect to controller using IP value
		/// </summary>
		/// <returns>True if connection is success</returns>
		public bool Connect()
		{
			//Already connected?
			if (IsConnected()) return true;
			ns = null;
			if (TestOnline())
			{
				try
				{
					if (client == null)
						client = new TcpClient(IP, 30004);

					if (!client.Connected)
						client.Connect(IP, 30004);

					if (client.Connected)
					{
						ns = client.GetStream();
						stopReading = false;
						Task.Run(() => Read());
						OnConnected?.Invoke(this, new EventArgs());
						return true;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}
			Disconnect();
			return false;
		}

		/// <summary>
		/// Read asynchronyously data received from Controller.
		/// </summary>
		private void Read()
		{
			Debug.Print("Start to Read at thread:" + Thread.CurrentThread.ManagedThreadId.ToString());
			while (!stopReading && (ns != null))
			{
				if (ns.DataAvailable)
				{
					Debug.Print("Something to Read");
					RTDE_Package newPackage = RTDE_Package.NewPackageFrom(ns);
					if (newPackage != null)
					{
						mut.WaitOne();
						switch (newPackage.Type)
						{
							case RTDE_Package.PackageType.RTDE_REQUEST_PROTOCOL_VERSION:
								{
									ProtoVersion = ((RTDE_Request_Protocol_Version)newPackage).Version;
									RecProtoVersionEvent?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_GET_URCONTROL_VERSION:
								{
									URCtrlV_Major = ((RTDE_Get_URControl_Version)newPackage).Major;
									URCtrlV_Minor = ((RTDE_Get_URControl_Version)newPackage).Minor;
									URCtrlV_Bugfix = ((RTDE_Get_URControl_Version)newPackage).Bugfix;
									URCtrlV_Build = ((RTDE_Get_URControl_Version)newPackage).Build;
									RecURCtrlVersionEvent?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_TEXT_MESSAGE:
								{
									LastMessage = ((RTDE_Text_Message)newPackage).Message;
									LastMessageSource = ((RTDE_Text_Message)newPackage).Source;
									RecMessageEvent?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_CONTROL_PACKAGE_SETUP_OUTPUTS:
								{
									LastOutSetupRecipeID = ((RTDE_Control_Package_Setup_Outputs)newPackage).RecipeID;
									LastOutSetupVarTypes = ((RTDE_Control_Package_Setup_Outputs)newPackage).Var_Types;

									//Check each type with LastRequest
									LastValidatedOutputs.Clear();
									if ( (LastOutSetupRecipeID != 0) && (LastRequestedOutputs.Count > 0) )
									{
										string[] varTypes = LastOutSetupVarTypes.Split(',');
										if (LastRequestedOutputs.Count == varTypes.Length)
										{
											int curs = 0;
											foreach (RTDE_Package.ControllerIOData data in LastRequestedOutputs)
											{
												RTDE_Package.DataType dtItem = data.Type;
												string sItemType = dtItem.GetAttribute<RTDE_Package.DataTypeAttribute>().Name;

												if (sItemType != varTypes[curs])
												{
													Debug.Print("Received bad type for index {0} in outputs setup.",curs);
													LastOutSetupRecipeID = 0;
													break;
												}
												++curs;
											}
										}
										else
										{
											Debug.Print("Received wrong number of type in outputs setup.");
											LastOutSetupRecipeID = 0;
										}
									}

									//All checks are OK, save List
									if (LastOutSetupRecipeID != 0)
										LastValidatedOutputs = new List<RTDE_Package.ControllerIOData>(LastRequestedOutputs);

									LastRequestedOutputs.Clear();
									RecCtrlPackSetupOut?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_CONTROL_PACKAGE_SETUP_INPUTS:
								{
									LastInSetupRecipeID = ((RTDE_Control_Package_Setup_Inputs)newPackage).RecipeID;
									LastInSetupVarTypes = ((RTDE_Control_Package_Setup_Inputs)newPackage).Var_Types;

									//Check each type with LastRequest
									LastValidatedInputs.Clear();
									if ((LastInSetupRecipeID != 0) && (LastRequestedInputs.Count > 0))
									{
										string[] varTypes = LastInSetupVarTypes.Split(',');
										if (LastRequestedInputs.Count == varTypes.Length)
										{
											int curs = 0;
											foreach (RTDE_Package.ControllerIOData data in LastRequestedInputs)
											{
												RTDE_Package.DataType dtItem = data.Type;
												string sItemType = dtItem.GetAttribute<RTDE_Package.DataTypeAttribute>().Name;

												if (sItemType != varTypes[curs])
												{
													Debug.Print("Received bad type for index {0} in inputs setup.", curs);
													LastInSetupRecipeID = 0;
													break;
												}
												++curs;
											}
										}
										else
										{
											Debug.Print("Received wrong number of type in inputs setup.");
											LastInSetupRecipeID = 0;
										}
									}

									//All checks are OK, save List
									if (LastInSetupRecipeID != 0)
										LastValidatedInputs = new List<RTDE_Package.ControllerIOData>(LastRequestedInputs);

									LastRequestedInputs.Clear();
									RecCtrlPackSetupIn?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_DATA_PACKAGE:
								{
									((RTDE_Data_Package)newPackage).GetData(ref LastValidatedOutputs);
									RecDataPackage?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_CONTROL_PACKAGE_START:
								{
									CtrlPackageRunning = ((RTDE_Control_Package_Start)newPackage).Accepted;
									RecCtrlPackStartDone?.Invoke(this, new EventArgs());
									break;
								}
							case RTDE_Package.PackageType.RTDE_CONTROL_PACKAGE_PAUSE:
								{
									CtrlPackageRunning &= !((RTDE_Control_Package_Pause)newPackage).Accepted;
									RecCtrlPackStartDone?.Invoke(this, new EventArgs());
									break;
								}
							default: break;
						}
						mut.ReleaseMutex();
					}
				}
			}
			stopReading = false;
			Debug.Print("Read Finish");
		}

		public event EventHandler OnDisconnected;
		/// <summary>
		/// Disconnect to controller and erase TCPCLient
		/// </summary>
		public void Disconnect()
		{
			if (ns != null)
			{
				stopReading = true;
				if (!SpinWait.SpinUntil(() => !stopReading, 500))
				{
					Debug.Print("Can't stop Reading task!!!");
				}
				ns.Close();
			}
			ns = null;
			if (client != null)
				if (client.Connected)
					client.Close();
			client = null;

			ProtoVersion = 0;
			RecProtoVersionEvent?.Invoke(this, new EventArgs());
			URCtrlV_Major = 0;
			URCtrlV_Minor = 0;
			URCtrlV_Bugfix = 0;
			URCtrlV_Build = 0;
			RecURCtrlVersionEvent?.Invoke(this, new EventArgs());
			LastMessageSource = "";
			LastMessage = "";
			RecMessageEvent?.Invoke(this, new EventArgs());
			LastRequestedOutputs.Clear();
			LastValidatedOutputs.Clear();
			CtrlPackageRunning = false;
			RecCtrlPackStartDone?.Invoke(this, new EventArgs());

			OnDisconnected?.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Ask for version of RTDE Protocol
		/// </summary>
		public void AskProtoVersion()
		{
			ProtoVersion = -1;

			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Request_Protocol_Version reqProtoVer = new RTDE_Request_Protocol_Version(2);
				reqProtoVer.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => ProtoVersion != -1, 1000);
				Disconnect();
			}
		}

		/// <summary>
		/// Ask for version of controller
		/// </summary>
		public void AskCtrlVersion()
		{
			URCtrlV_Major = 0; URCtrlV_Minor = 0; URCtrlV_Bugfix = 0; URCtrlV_Build = 0;

			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Get_URControl_Version getURControlVersion = new RTDE_Get_URControl_Version();
				getURControlVersion.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => URCtrlV_Major != 0, 1000);
				Disconnect();
			}
		}

		/// <summary>
		/// Send a message to controller
		/// </summary>
		/// <param name="message">Message to send</param>
		/// <param name="source">Source of message</param>
		public void SendMessage(string message, string source = "", byte warnLevel = 0)
		{
			LastMessageSource = ""; LastMessage = "";

			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Text_Message textMessage = new RTDE_Text_Message
				{
					Message = message,
					Source = source,
					WarningLevel = warnLevel
				};
				textMessage.SendTo(ns);
			}
			if (!wasConnected)
				Disconnect();

		}

		/// <summary>
		/// Setup Control Package for output. Return list of variables type if OK, else "NOT_FOUND".
		/// </summary>
		/// <param name="frequency">The frequency must be between 1 and 125 Hz and the output rate will be according to floor(125 / frequency).</param>
		/// <param name="varNames">The variable names is a list of comma separated variable name strings.</param>
		public void CtrlPackageSetupOutputs(double frequency, string varNames)
		{
			LastOutSetupRecipeID = 0; LastOutSetupVarTypes = ""; LastRequestedOutputs.Clear(); LastValidatedOutputs.Clear();

			bool wasConnected = IsConnected();
			if (Connect())
			{
				//Save Requested output list to check it when receive acknoledge
				string[] aVarNames = varNames.Split(',');
				foreach (string varName in aVarNames)
				{
					RTDE_Package.ControllerIOData ctrlOut = RTDE_Package.ControllerOutput.Find(x => x.Name == varName);
					if (ctrlOut != null)
						LastRequestedOutputs.Add(ctrlOut);
				}

				RTDE_Control_Package_Setup_Outputs ctrlPackSetOut = new RTDE_Control_Package_Setup_Outputs((ushort)this.ProtoVersion)
				{
					OutputFreq = frequency,
					Var_Names = varNames,
				};
				ctrlPackSetOut.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => LastOutSetupRecipeID != 0, 1000);
				Disconnect();
			}

		}

		/// <summary>
		/// Setup Control Package for input. Return list of variables type if OK, else "NOT_FOUND" or "IN_USE".
		/// </summary>
		/// <param name="varNames">The variable names is a list of comma separated variable name strings.</param>
		public void CtrlPackageSetupInputs(string varNames)
		{
			LastInSetupRecipeID = 0; LastInSetupVarTypes = ""; LastRequestedInputs.Clear(); LastValidatedInputs.Clear();

			bool wasConnected = IsConnected();
			if (Connect())
			{
				//Save Requested input list to check it when receive acknoledge
				string[] aVarNames = varNames.Split(',');
				foreach (string varName in aVarNames)
				{
					RTDE_Package.ControllerIOData ctrlIn = RTDE_Package.ControllerInput.Find(x => x.Name == varName);
					if (ctrlIn != null)
						LastRequestedInputs.Add(ctrlIn);
				}

				RTDE_Control_Package_Setup_Inputs ctrlPackSetIn = new RTDE_Control_Package_Setup_Inputs()
				{
					Var_Names = varNames,
				};
				ctrlPackSetIn.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => LastInSetupRecipeID != 0, 1000);
				Disconnect();
			}

		}

		/// <summary>
		/// Ask to Controller to start sending output updates.
		/// </summary>
		public void AskCtrlStartOutput()
		{
			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Control_Package_Start ctrlPackStart = new RTDE_Control_Package_Start();
				ctrlPackStart.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => CtrlPackageRunning, 1000);
				//Don't disconnect
				//Disconnect();
			}
		}

		/// <summary>
		/// Ask to Controller to pause sending output updates.
		/// </summary>
		public void AskCtrlPauseOutput()
		{
			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Control_Package_Pause ctrlPackStart = new RTDE_Control_Package_Pause();
				ctrlPackStart.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => !CtrlPackageRunning, 1000);
				Disconnect();
			}
		}

		/// <summary>
		/// Send Inputs values to controller
		/// </summary>
		/// <param name="ctrlIODatas">List of ControllerIOData with values to send.</param>
		public void CtrlPackageSendInputs(List<RTDE_Package.ControllerIOData> ctrlIODatas)
		{
			bool wasConnected = IsConnected();
			if (Connect())
			{
				RTDE_Data_Package dataPackage = new RTDE_Data_Package(LastInSetupRecipeID);
				dataPackage.SetData(ctrlIODatas);
				dataPackage.SendTo(ns);
			}
			if (!wasConnected)
			{
				//If not connected, waiting for result
				SpinWait.SpinUntil(() => CtrlPackageRunning, 1000);
				//Don't disconnect
				//Disconnect();
			}
		}

	}
}
