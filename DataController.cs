using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DataController : MonoBehaviour {
    /// <summary>
    /// If true, will write to a Google Sheet instead of to a CSV file
    /// </summary>
    [Tooltip("If true, will write to a Google Sheet instead of to a CSV file")]
    public bool writeToGoogleSheet;
    /// <summary>
    /// Component used to write to a Google Sheet
    /// </summary>
    [Tooltip("Component used to write to a Google Sheet")]
    public GoogleSheetValues googleSheetValuesLooks;
    public GoogleSheetValues googleSheetValuesSession;
    public void AddLooksLine(string objectName, float timePeriphery, float timeCenter, float sizePeriphery, float sizeCenter)
    {
        googleSheetValuesLooks.ChangeRowSize(googleSheetValuesLooks.rows.Count + 1);
        googleSheetValuesLooks.ChangeGoogleValue(googleSheetValuesLooks.rows.Count, 3, objectName);
        googleSheetValuesLooks.ChangeGoogleValue(googleSheetValuesLooks.rows.Count, 4, timeCenter + "");
        googleSheetValuesLooks.ChangeGoogleValue(googleSheetValuesLooks.rows.Count, 5, sizeCenter + "");
        googleSheetValuesLooks.ChangeGoogleValue(googleSheetValuesLooks.rows.Count, 6, timePeriphery + "");
        googleSheetValuesLooks.ChangeGoogleValue(googleSheetValuesLooks.rows.Count, 7, sizePeriphery + "");
    }
    public void AddSessionLine(string objectName, float timePeriphery, float timeCenter, float sizePeriphery, float sizeCenter)
    {
        googleSheetValuesSession.ChangeGoogleValue(1, 3, float.Parse(googleSheetValuesSession.rows[0].columns[2]) + timeCenter + "");
        googleSheetValuesSession.ChangeGoogleValue(1, 3, float.Parse(googleSheetValuesSession.rows[0].columns[3]) + timePeriphery + "");
        googleSheetValuesSession.ChangeGoogleValue(1, 3, float.Parse(googleSheetValuesSession.rows[0].columns[4]) + sizeCenter + "");
        googleSheetValuesSession.ChangeGoogleValue(1, 3, float.Parse(googleSheetValuesSession.rows[0].columns[5]) + sizePeriphery + "");
    }
    void Save()
    {
        //loop through all the stims and add their data to the data for this session
        DataTrackingObject[] dataTrackingObjects = GameObject.FindObjectsOfType<DataTrackingObject>();
        foreach(DataTrackingObject dto in dataTrackingObjects)
        {
            AddSessionLine(dto.name, dto.timePeriphery, dto.timeCenter, dto.sizePeriphery, dto.sizeCenter);
        }
        // Write to a Google Sheet
        if (writeToGoogleSheet)
        {
            googleSheetValuesLooks.AppendGoogleValues();
            googleSheetValuesSession.AppendGoogleValues();
        }
        // Write to a CSV file
        else
        {
            string delimiter = ",";

            StringBuilder sb = new StringBuilder();

            foreach (GoogleSheetValues.Data d in googleSheetValuesLooks.rows)
            {
                foreach (String s in d.columns)
                {
                    sb.AppendLine(string.Join(delimiter, s));
                }
                sb.AppendLine("\n");
            }

            //TODO: add filename to path below
            string filePath = Application.dataPath;

            StreamWriter outStream = System.IO.File.CreateText(filePath);
            outStream.WriteLine(sb);
            outStream.Close();

            sb.Clear();
            foreach (GoogleSheetValues.Data d in googleSheetValuesSession.rows)
            {
                foreach (String s in d.columns)
                {
                    sb.AppendLine(string.Join(delimiter, s));
                }
                sb.AppendLine("\n");
            }
            //TODO: new filename for the session data below
            outStream = System.IO.File.CreateText("");
            outStream.WriteLine(sb);
            outStream.Close();
        }
    }
}
