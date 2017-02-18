using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using System.Runtime.InteropServices;
using Extensibility;
using Microsoft.Office.Interop.OneNote;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TodayNote
{
    [Guid("81E68F46-AA03-45A8-BE8F-A8AB5AF99664"), ProgId("TodayNote.TodayNoteAddIn")]
    public class TodayNoteAddIn : IDTExtensibility2
    {
        IApplication app;

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
            Debug.Print("OnBeginShutdown");
            app = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void OnConnection(object application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            Debug.Print("OnConnection {0}", ConnectMode);
            app = application as IApplication;

            Do();
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            Debug.Print("OnDisconnection {0}", RemoveMode);
            app = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void OnStartupComplete(ref Array custom)
        {
            Debug.Print("OnStartupComplete");
        }

        public void Do()
        {
            Debug.Print("Do()");

            EventWaitHandle waitHandle;
            if (!EventWaitHandle.TryOpenExisting("TodayNote", out waitHandle)) return;

            waitHandle.Set();
            
            try
            {
                string hierarchyXML;
                app.GetHierarchy("", HierarchyScope.hsNotebooks, out hierarchyXML);

                XNamespace one = "http://schemas.microsoft.com/office/onenote/2013/onenote";
                XDocument doc = XDocument.Parse(hierarchyXML);

                // one:Notebook element를 찾는다
                var noteBookID = doc.Descendants(one + "Notebook")
                                    .Where(elem => (string)elem.Attribute("name") == "기본")
                                    .Select(elem => (string)elem.Attribute("ID"))
                                    .FirstOrDefault();

                app.GetHierarchy(noteBookID, HierarchyScope.hsSections, out hierarchyXML);
                doc = XDocument.Parse(hierarchyXML);
                var sectionID = doc.Descendants(one + "Section")
                                   .Where(elem => (string)elem.Attribute("name") == "2017년")
                                   .Select(elem => (string)elem.Attribute("ID"))
                                   .FirstOrDefault();

                var dateString = DateTime.Today.ToString("yyyy-MM-dd");

                app.GetHierarchy(sectionID, HierarchyScope.hsPages, out hierarchyXML);
                doc = XDocument.Parse(hierarchyXML);
                var pageID = doc.Descendants(one + "Page")
                                .Where(elem => ((string)elem.Attribute("name")).StartsWith(dateString))
                                .Select(elem => (string)elem.Attribute("ID"))
                                .FirstOrDefault();

                if (string.IsNullOrEmpty(pageID))
                {
                    app.CreateNewPage(sectionID, out pageID, NewPageStyle.npsBlankPageWithTitle);

                    var pageContent = string.Format(@"<one:Page xmlns:one=""http://schemas.microsoft.com/office/onenote/2013/onenote"" ID=""{0}"">
  <one:Title>
    <one:OE>
      <one:T><![CDATA[{1}]]></one:T>
    </one:OE>
  </one:Title>
</one:Page>", pageID, dateString);

                    app.UpdatePageContent(pageContent);
                    


                    // app.GetHierarchy(pageID, HierarchyScope.hsSelf, out hierarchyXML);

                    /*doc = XDocument.Parse(hierarchyXML);
                    Debug.Print(hierarchyXML);

                    var pageElem = doc.Descendants(one + "Page").FirstOrDefault();
                    if (pageElem != null)
                    {
                        var nameAttr = pageElem.Attribute("name");
                        if (nameAttr != null)
                        {
                            nameAttr.Value = dateString + ", Success";
                            Debug.Print("UpdateHierarchy");
                            Debug.Print(doc.ToString());
                            app.UpdatePageContent(doc.ToString());
                        }
                    }*/
                }

                app.NavigateTo(pageID);

                // app.NavigateTo(pageID);
                // app.GetPageContent(pageID, out hierarchyXML);
                // app.UpdatePageContent()

            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}
