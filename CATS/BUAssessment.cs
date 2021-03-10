using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace CATS
{
    public class BUAssessment
    {
        public BUAssessment()
        {
            setDefaultValues();
        }

        private void setDefaultValues()
        {
            this.createdDate = DateTime.Now;
            this.templateVer = 19.0f; //Hardcoded single template at the moment

            //TitleLevelPage
            this.unitTitle = String.Empty;
            this.asmtTitle = String.Empty;
            this.level = 4;
            this.isResub = false;
            this.asmtNoX = 1;
            this.asmtNoY = 2;

            //WeightDatePage
            this.isGroup = false;
            this.weighting = 50;
            this.creditValue = 20;
            this.submissionDueDate = new DateTime(1999, 1, 19, 19, 19, 19); //Arbitrary initialisation date to ignore
            this.impPath = String.Empty;
        }

        /// <summary>
        /// Checks whether the submission due date is greater than the initialisation date (of 1999)
        /// </summary>
        /// <returns>True if the submission due date is greater, false otherwise</returns>
        public bool hasValidSubmissionDueDate()
        {
            DateTime initialisationDate = new DateTime(1999, 1, 19, 19, 19, 19); //Arbitrary initialisation date to ignore
            if (this.submissionDueDate > initialisationDate) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Populates (loads) this object from a specified JSON format filepath
        /// </summary>
        /// <param name="filePath">The full filepath to load</param>
        public void loadFromJson(string filePath)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, System.IO.FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                    Console.WriteLine("Successfully loaded " + filePath);
                }
                string json = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();

                JsonConvert.PopulateObject(json, this); //Set the values of this BUAssessment instance to match those loaded from the JSON file
            }
            catch (Exception)
            {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to load " + filePath + " - used by another process?");
                MessageBox.Show("Unable to load " + filePath + ". It might be locked by another process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Saves this object in JSON format to the specified filepath
        /// </summary>
        /// <param name="filepath">The full filepath to save/overwrite</param>
        public void saveAsJson(string filePath)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    Console.WriteLine("Successfully saved " + filePath);
                    ms.Close();
                }
            }
            catch (Exception)
            {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + filePath + " - used by another process?");
                MessageBox.Show("Unable to save " + filePath + ". It might be locked by another process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DateTime createdDate { get; set; }
        public float templateVer { get; set; }

        //TitleLevelPage
        public string unitTitle { get; set; }
        public string asmtTitle { get; set; }
        public int level { get; set; }
        public bool isResub { get; set; }
        public int asmtNoX { get; set; }
        public int asmtNoY { get; set; }

        //WeightDatePage
        public bool isGroup { get; set; }
        public int weighting { get; set; }
        public int creditValue { get; set; }
        public DateTime submissionDueDate { get; set; }
        public string impPath { get; set; }
    }
}
