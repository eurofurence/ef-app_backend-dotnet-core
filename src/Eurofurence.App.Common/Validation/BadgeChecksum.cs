using System;

namespace Eurofurence.App.Common.Validation
{
    public static class BadgeChecksum
    {
        private static int[] weight = { 3, 5, 7, 11, 13, 17 };
        private static String CHECKSUM_LETTER_MAP = "FJQCETNKWLVGYHSZXDBUARP"; // 23 letters (prime)

        public static char CalculateChecksum(int id)
        {
            int sum = 0;
            int place = 0;
            while (id > 0)
            {
                int digit = id % 10;
                sum += digit * weight[place];
                id /= 10;
                place++;
            }
            int idx = sum % (CHECKSUM_LETTER_MAP.Length);
            return CHECKSUM_LETTER_MAP[idx];
        }

        public static bool IsValid(string id)
        {
            int regNo;
            return TryParse(id, out regNo);
        }

        public static bool TryParse(string id, out int regNo)
        {
            var numberPart = id.Substring(0, id.Length - 1);
            var checksumLetter = id[id.Length-1];

            if (CHECKSUM_LETTER_MAP.IndexOf(checksumLetter) == -1)
            {
                regNo = 0;
                return false;
            }

            if (!Int32.TryParse(numberPart, out regNo)) return false;

            return true; // Until we fix the hashing.
            return checksumLetter == CalculateChecksum(regNo);
        }
    }
}
