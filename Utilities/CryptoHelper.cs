using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WindowLocker.Utilities
{
    /// <summary>
    /// 문자열 암호화 및 복호화를 위한 유틸리티 클래스
    /// </summary>
    public static class CryptoHelper
    {
        // 암호화에 사용될 엔트로피 (추가 보안용)
        // 실제 앱에서는 이 값도 보호해야 합니다
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("WindowLockerSecuritySalt");

        /// <summary>
        /// 문자열을 Windows의 Data Protection API(DPAPI)를 사용하여 암호화합니다.
        /// </summary>
        /// <param name="plainText">암호화할 문자열</param>
        /// <returns>Base64로 인코딩된 암호화된 문자열</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                // 문자열을 바이트 배열로 변환
                byte[] plainBytes = Encoding.Unicode.GetBytes(plainText);
                
                // DPAPI를 사용하여 데이터 보호
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes, 
                    entropy,
                    DataProtectionScope.CurrentUser);
                
                // 암호화된 바이트 배열을 Base64 문자열로 변환
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                // 암호화 실패 시 빈 문자열 반환
                System.Diagnostics.Debug.WriteLine($"암호화 실패: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 암호화된 문자열을 Windows의 Data Protection API(DPAPI)를 사용하여 복호화합니다.
        /// </summary>
        /// <param name="encryptedText">복호화할 Base64로 인코딩된 문자열</param>
        /// <returns>복호화된 원본 문자열</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                // Base64 문자열을 바이트 배열로 변환
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                
                // DPAPI를 사용하여 데이터 복호화
                byte[] plainBytes = ProtectedData.Unprotect(
                    encryptedBytes, 
                    entropy,
                    DataProtectionScope.CurrentUser);
                
                // 복호화된 바이트 배열을 문자열로 변환
                return Encoding.Unicode.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                // 복호화 실패 시 빈 문자열 반환
                System.Diagnostics.Debug.WriteLine($"복호화 실패: {ex.Message}");
                return string.Empty;
            }
        }
    }
} 