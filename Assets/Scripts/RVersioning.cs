using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal static class RVersioning
    {
        private static (int, int) GetVersionParts()
        {
            var vStr = Application.version;
            var parts = vStr.Split(".");
            if (parts.Length == 2)
            {
                // TryParse returns 'true' and sets the output variable on success, 'false' otherwise.
                bool success1 = int.TryParse(parts[0], out int firstNumber);
                bool success2 = int.TryParse(parts[1], out int secondNumber);
                if (!success1 || !success2)
                {
                    Debug.LogError("One or both parts could not be converted to an integer.");
                    return (0, 0);
                }
                return (firstNumber, secondNumber);
            }
            else
            {
                Debug.LogError($"bad version format on {vStr}");
                return (0, 0);
            }
        }
        public static int GetProtocolNum()
        {
            (int p, _) = GetVersionParts();
            return p;
        }
        public static int GetVersionNum()
        {
            (_, int v) = GetVersionParts();
            return v;
        }
        public static string GetVersionStr()
        {
            (int p, int v) = GetVersionParts();
            return $"{p}.{v}";
        }
    }
}
