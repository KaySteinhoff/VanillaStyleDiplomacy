using System;
using System.IO;
using System.Text;
using TaleWorlds.Library;
using VanillaStyleDiplomacy.Helpers;

namespace VanillaStyleDiplomacy.Managers
{
    public class LoggingManager
    {
        private static LoggingManager instance = null;
        private StreamWriter logFile = null;

        public static LoggingManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new LoggingManager();
                return instance;
            }
        }

        private LoggingManager()
        {
            try
            {
                if (!Directory.Exists($"{ConstantsHelper.ModuleFolder}/Logs"))
                    Directory.CreateDirectory($"{ConstantsHelper.ModuleFolder}/Logs");
                logFile = new StreamWriter($"{ConstantsHelper.ModuleFolder}/Logs/{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", false, Encoding.UTF8);
            }
            catch
            {
                // IDK what to even do here
                logFile = null;
            }
            finally
            {
                LogMessage($"'LoggingManager' of '{ConstantsHelper.Name}' successfully initialized!");
            }
        }

        public bool LogMessage(string message)
        {
            if (VanillaStyleDiplomacyModule.Loaded)
                InformationManager.DisplayMessage(new InformationMessage(message));

            if (logFile == null)
                return false;

            try
            {
                logFile.WriteLine($"[{DateTime.Now.TimeOfDay.ToString("h\\:m\\:s")}] {message}");
                logFile.Flush();
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return true;
        }

        public bool LogException(Exception exception)
        {
            if (VanillaStyleDiplomacyModule.Loaded)
                InformationManager.DisplayMessage(new InformationMessage($"{exception.GetType()}: {exception.Message}", Color.FromUint(0x00ff0000)));

            if (logFile == null)
                return false;

            try
            {
                logFile.WriteLine($"{exception.GetType()}: '{exception.Message}'\n\tStacktrace:\n{exception.StackTrace}");
                logFile.Flush();
            } catch { }
            return true;
        }

        public void Dispose()
        {
            if (logFile == null)
                return;
            LogMessage("Terminating...");
            logFile.Close();
        }
    }
}