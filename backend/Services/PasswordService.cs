using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordService
{
   
    public static string HashPassword(string password)
    {
        //случайная соль (16 байт)
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        //хэширование пароля через статический метод PBKDF2 
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,                    // Пароль
            salt,                        // Соль
            100000,                      // Итерации
            HashAlgorithmName.SHA256,   // Алгоритм
            32                           // Длина хеша в байтах (256 бит)
        );

        // 3.соль + хеш(16 + 32 = 48 байт)
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        // 4. Конвертируем в строку Base64 для хранения в БД
        return Convert.ToBase64String(hashBytes);
    }
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            //декодирование из Base64
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            //проверка длины на 48
            if (hashBytes.Length != 48)
                return false;

            //извлечение соли(16 бит)
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // сохранение хеша (32 байта)
            byte[] savedHash = new byte[32];
            Array.Copy(hashBytes, 16, savedHash, 0, 32);

            // хэширование введённого пароль с прежней солью
            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            //сравнение хешей безопасно
            return CryptographicOperations.FixedTimeEquals(computedHash, savedHash);
        }
        catch
        {
            return false;
        }
    }
    public static async Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
{
    //300 мс — защита от атак по времени
    await Task.Delay(300);
    
    return VerifyPassword(password, hashedPassword);
}
}