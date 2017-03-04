namespace TouchpadPeaceFree
{
    using System;

    internal class ProgramSettingsData
    {
        internal int FilterCount { get; set; }
        internal DateTime DateLastUpdated { get; set; }

        internal ProgramSettingsData()
        {
            FilterCount = 0;
        }

        internal static ProgramSettingsData fromString(string s)
        {
            string[] parts = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int filterCount = 0;
            DateTime lastCheckDate = new DateTime(2015, 1, 1);

            if (parts != null && parts.Length == 2)
            {
                Int32.TryParse(parts[0], out filterCount);
                DateTime.TryParseExact(
                    parts[1], 
                    "yyyy-MM-dd", 
                    null/*provider*/, 
                    System.Globalization.DateTimeStyles.None, 
                    out lastCheckDate);
            }

            ProgramSettingsData returnValue = new ProgramSettingsData
            {
                FilterCount = filterCount,
                DateLastUpdated = lastCheckDate
            };

            return returnValue;
        }

        public override string ToString()
        {
            string returnValue = string.Format("{0},{1:yyyy-MM-dd}",
                this.FilterCount, DateLastUpdated.Year, DateLastUpdated.Month,
                DateLastUpdated.Day);

            return returnValue;
        }
    }

}
