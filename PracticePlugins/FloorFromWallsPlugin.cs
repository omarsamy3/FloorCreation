using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PracticePlugins
{
    public class FloorFromWallsPlugin : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        //Get the current assemply location
        Assembly assemply = Assembly.GetExecutingAssembly();

        //The tab plugin name.
        string TabName = "FloorFromWalls";

        public Result OnStartup(UIControlledApplication application)
        {
            //Create the tab for the plugin.
            application.CreateRibbonTab(TabName);

            //Create the ribbon pannel for the floor creation plugin.
            RibbonPanel FloorPanel = application.CreateRibbonPanel(TabName, "Floor");

            //Create the plugin button.
            PushButton btn = CreateButton(FloorPanel, "Create", "Create a floor from closed walls", typeof(CreateFloorFromWallsContiguous).FullName,
                                          $"{nameof(PracticePlugins)}.Resources.Floor.png", "Create a floor from selected closed walls with all cases");

            return Result.Succeeded;
        }

        //This function is to create the plugin button with all details.
        public PushButton CreateButton( RibbonPanel panel, string ButtonName, string ButtonText, string ClassName, string ImgResource, string description = null)
        {
            //Create the Plugin putton data
            PushButtonData BtnData = new PushButtonData(ButtonName, ButtonText, assemply.Location, ClassName);

            //Create the plugin button.
            PushButton PluginBtn = panel.AddItem(BtnData) as PushButton;

            //Add a tool tip description if the user send it
            if(description != null) PluginBtn.ToolTip = description;

            //Initialize the image
            BitmapImage img = new BitmapImage();

            //Intialize the stream and assign the empedded resource to it.
            Stream stream = assemply.GetManifestResourceStream(ImgResource);

            //Assign the resource stream to the image resource stream.
            try
            {
                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();
            }
            catch(Exception e)
            {
                //Show the error message in the task dialog.
                TaskDialog.Show("Creation Button Error", e.Message);
            }

            //Push the image to the button.
            PluginBtn.LargeImage = img;

            //Return the button
            return PluginBtn;

        }
    }
}
