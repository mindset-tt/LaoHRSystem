using System.Text;

namespace LaoHR.API.Services;

public static class LaoNumberUtils
{
    // 0-9
    private static readonly string[] Digits = { "ສູນ", "ໜຶ່ງ", "ສອງ", "ສາມ", "ສີ່", "ຫ້າ", "ຫົກ", "ເຈັດ", "ແປດ", "ເກົ້າ" };

    public static string NumberToKipWords(decimal amount)
    {
        if (amount == 0) return "ສູນກີບຖ້ວນ";
        
        long integerPart = (long)amount;
        var sb = new StringBuilder();
        
        sb.Append(Convert(integerPart));
        sb.Append("ກີບຖ້ວນ");
        
        return sb.ToString();
    }
    
    private static string Convert(long number)
    {
        if (number >= 1000000000)
        {
            return Convert(number / 1000000000) + "ຕື້" + Convert(number % 1000000000);
        }
        if (number >= 1000000)
        {
            return Convert(number / 1000000) + "ລ້ານ" + Convert(number % 1000000);
        }
        if (number >= 100000)
        {
            return Convert(number / 100000) + "ແສນ" + Convert(number % 100000);
        }
        if (number >= 10000)
        {
            return Convert(number / 10000) + "ໝື່ນ" + Convert(number % 10000);
        }
        if (number >= 1000)
        {
            return Convert(number / 1000) + "ພັນ" + Convert(number % 1000);
        }
        if (number >= 100)
        {
            return Convert(number / 100) + "ຮ້ອຍ" + Convert(number % 100);
        }
        if (number >= 10)
        {
            if (number == 10) return "ສິບ";
            if (number == 11) return "ສິບເອັດ";
            
            long tens = number / 10;
            long unit = number % 10;
            
            string tenStr = (tens == 1) ? "ສິບ" : (tens == 2 ? "ຊາວ" : Digits[tens] + "ສິບ");
            string unitStr = (unit == 0) ? "" : ((unit == 1 && tens > 0) ? "ເອັດ" : Digits[unit]); // 21 -> Sao Et
            
            return tenStr + unitStr;
        }
        
        // 0-9 (but recursive calls handle 0 explicitly)
        if (number == 0) return ""; 
        return Digits[number];
    }
}
