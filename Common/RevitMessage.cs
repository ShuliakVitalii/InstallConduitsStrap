using Autodesk.Revit.UI;

namespace InstallConduitsStrap.Common
{
    public class RevitMessage
    {
        /// <summary>
        /// Display the specify message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        #region public methods
        public static void Display(string message, WindowType type)
        {
            string title = "";
            var icon = TaskDialogIcon.TaskDialogIconNone;

            //Customize window based on type of message.
            switch (type)
            {
                case WindowType.Information:
                    title = "~ INFORMATION ~";
                    icon = TaskDialogIcon.TaskDialogIconInformation;
                    break;
                case WindowType.Warning:
                    title = "~ WARNING ~";
                    icon = TaskDialogIcon.TaskDialogIconWarning;
                    break;
                case WindowType.Error:
                    title = "~ ERROR ~";
                    icon = TaskDialogIcon.TaskDialogIconError;
                    break;
                default:
                    break;
            }
            // Construction window to display specified message.
            var window = new TaskDialog(title)
            {
                MainContent = message,
                MainIcon = icon,
                CommonButtons = TaskDialogCommonButtons.Ok
            };            
            window.Show();
        }

        #endregion
    }
}