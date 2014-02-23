using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace PoleSagTool
{
    public class XData
    {
        DBObject _obj;
        string _appName;
        string RegisteredAppName { get { return _appName; } }

        public XData(DBObject obj, string appName)
        {
            _obj = obj;
            _appName = appName;
        }

        public List<string> GetAppDataList()
        {
            ResultBuffer resbuf = _obj.GetXDataForApplication(RegisteredAppName);
            var result = new List<string>();
            if (resbuf == null) return result;

            foreach (TypedValue listEntry in resbuf.AsArray())
            {
                if (listEntry.TypeCode != (short)DxfCode.ExtendedDataAsciiString) continue;
                result.Add(listEntry.Value.ToString());
            }
            return result;
        }

        public Dictionary<string, object> GetAppData()
        {
            ResultBuffer resbuf = _obj.GetXDataForApplication(RegisteredAppName);
            var result = new Dictionary<string, object>();
            if (resbuf == null) return result;

            foreach (string pair in GetAppDataList())
            {
                int equalsIndex = pair.IndexOf("=");
                string fieldName = pair.Substring(0, equalsIndex);
                if (result.ContainsKey(fieldName)) continue;
                string fieldValue = pair.Substring(equalsIndex + 1);
                result.Add(fieldName, fieldValue);
            }

            return result;
        }

        public void SetAppDataList(IList<string> data)
        {
            EnsureAppRegistered();

            // Create new resbuf with new data and put back in Xrecord
            var newRb = new ResultBuffer();
            newRb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegisteredAppName));
            if (data.Count > 0)
            {
                newRb.Add(new TypedValue((int)DxfCode.ExtendedDataControlString, "{"));
                foreach (string datum in data)
                {
                    newRb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, datum));
                }
                newRb.Add(new TypedValue((int)DxfCode.ExtendedDataControlString, "}"));
            }
            _obj.XData = newRb;
        }

        public void SetAppData(Dictionary<string, object> data)
        {
            var dataList = new List<string>(data.Count);
            foreach (KeyValuePair<string, object> datum in data)
            {
                dataList.Add(string.Format("{0}={1}", datum.Key, datum.Value));
            }
            SetAppDataList(dataList);
        }

        void EnsureAppRegistered()
        {
            //Check if "SNS_NuDes" is registered app for XData or not
            using (Transaction trans = AcadApp.TM.StartTransaction())
            {
                RegAppTable appTable = trans.GetObject(
                    AcadApp.DB.RegAppTableId, OpenMode.ForWrite) as RegAppTable;
                bool isAppRegistered = false;
                foreach (ObjectId appID in appTable)
                {
                    RegAppTableRecord app = trans.GetObject(
                        appID, OpenMode.ForRead) as RegAppTableRecord;
                    if (app.Name == RegisteredAppName)
                    {
                        isAppRegistered = true;
                        break;
                    }
                }

                //Create RegAppTableRecord, if needed
                if (!isAppRegistered)
                {
                    RegAppTableRecord appTableRecord = new RegAppTableRecord();
                    appTableRecord.Name = RegisteredAppName;
                    appTable.Add(appTableRecord);
                    trans.AddNewlyCreatedDBObject(appTableRecord, true);
                }
                trans.Commit();
            }
        }
    }
}
