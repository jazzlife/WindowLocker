using System;
using System.Security.Cryptography;
using System.Text;

namespace WindowLocker.Utilities
{
    /// <summary>
    /// 모든 환경에서 일관된 해시값을 생성하는 유틸리티 클래스
    /// </summary>
    public static class HashGenerator
    {
        /// <summary>
        /// 비밀번호 문자열을 표준화된 방식으로 SHA-256 해시로 변환합니다.
        /// 이 메서드는 모든 환경에서 동일한 입력에 대해 동일한 결과를 반환합니다.
        /// </summary>
        /// <param name="password">해시로 변환할 비밀번호</param>
        /// <returns>SHA-256 해시 문자열(소문자 16진수)</returns>
        public static string ComputePasswordHash(string password)
        {
            // UTF-8은 가장 표준적인 인코딩이므로 항상 이것을 사용
            return ComputeSha256Hash(password, Encoding.UTF8);
        }

        /// <summary>
        /// 문자열의 SHA256 해시를 계산합니다. 인코딩을 지정할 수 있습니다.
        /// </summary>
        private static string ComputeSha256Hash(string rawData, Encoding encoding)
        {
            if (string.IsNullOrEmpty(rawData))
                return string.Empty;
                
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(encoding.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 