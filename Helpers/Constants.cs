﻿using System.IO;
using System.Reflection;

namespace AdvansysPOC
{
    public static class Constants
    {
        //Parameters
        public const string GenericFamilyName = "Straight";
        public static string ConveyorNumber = "Conveyor_Number";
        public static string ConveyorHand = "Conveyor_Hand";
        public static string LastUnitId = "NextUnitId";
        public static string Conveyor_Elevation_In = "Conveyor_Elevation_In";
        public static string Conveyor_Envelop = "Load_Envelope";
        public static string Conveyor_ZoneLength = "Zone_Length";
        public static string Conveyor_ZoneLengthinBeds = "CLR_ZONES";
        public static string Bed_Length = "Bed_Length";
        public static string Bed_Width = "Bed_Width";
        //public static string DriveBed_Speed = "CLR_SPEED_FPM";
        public const string Roller_CenterToCenter = "CLR_ROLLER_CENTER";
        public const string Drive_Speed = "CLR_SPEED_FPM";
        public const string Center_Drive = "Center_Drive";
        public const string HP = "HP";
        public const string GR_Height = "GR_HEIGHT";
        public const string Conveyor_Speed = "Speed";
        public const string HasHungerSupport = "CEILING_SUPPORT";

        public static string EntranceBedFamilyName = "C380_ENTRY";
        public static string EntranceBedFileName = "C380_ENTRY.rfa";
        public static string BrakeBedFamilyName = "C353";
        public static string BrakeBedFileName = "C353.rfa";
        public static string ExitBedFamilyName = "C380_EXIT";
        public static string ExitBedFileName = "C380_EXIT.rfa";
        public static string CTFFamilyName = "C351";
        public static string CTFFamilyFileName = "C351.rfa";
        public static string IntermediateFamilyName = "C352";
        public static string IntermediateFamilyFileName = "C352.rfa";
        public static string DriveFamilyName = "C370";
        public static string DriveFamilyFileName = "C370.rfa";
        public static string SupportFamilyFileName = "C2101.rfa";
        public static string SupportFamilyName = "C2101";
        public static string LongSupportFamilyFileName = "C2201.rfa";
        public static string LongSupportFamilyName = "C2201";
        public static string HangerSupportFamilyFileName = "C2112.rfa";
        public static string HangerSupportFamilyName = "C2112";
        public static string GuideRailFamilyFileName = "C2000.rfa";
        public static string GuideRailFamilyName = "C2000";

        //Controls families
        public const string PEMFamilyName = "PEM";
        public const string PEMFamilyFileName = "PEM.rfa";
        public const string MotorFamilyName = "Motor";
        public const string MotorFamilyFileName = "Motor.rfa";
        public const string DISCFamilyName = "DISC";
        public const string DISCFamilyFileName = "DISC.rfa";
        public const string VFDFamilyName = "VFD";
        public const string VFDFamilyFileName = "VFD.rfa";
        public const string ZIMFamilyName = "ZIM";
        public const string ZIMFamilyFileName = "ZIM.rfa";
        public const string PowerSupplyFamilyName = "C389";
        public const string PowerSupplyFamilyFileName = "C389.rfa";
        public const string SolenoidFamilyName = "SOL";
        public const string SolenoidFamilyFileName = "SOL.rfa";

        public const int MaxZonesPerPowerSupply = 33;
    }
}
