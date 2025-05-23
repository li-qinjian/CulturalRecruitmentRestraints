﻿using System;
using System.IO;

namespace CulturalRecruitmentRestraints.Utils
{
    class Logging
    {
        public static string PrePrend = "";

        public static void Lm(string message)
        {
            try
            {
                using StreamWriter sw = File.AppendText(Statics.logPath);
                sw.WriteLine(PrePrend + " : " + DateTime.Now.ToString() + " : " + message + "\r\n");
            }
            catch (Exception ex)
            {
                IM.ShowError("C.R.R Logging error", "Logging", ex);
            }
        }

    }
}
