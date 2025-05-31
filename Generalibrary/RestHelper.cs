using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using Generalibrary;

namespace BitcoinChecker
{
    public enum ERestMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Head,
        Options,
        Patch,
        Merge,
        Copy,
        Search
    }

    public class RestHelper : IniHelper
    {
        // ====================================================================
        // ENUM
        // ====================================================================

        /// <summary>
        /// 거래소 enum
        /// </summary>
        public enum EExchange
        {
            Upbit,
            Bithumb
        }


        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// system.ini path
        /// </summary>
        private const string INI_PATH = "system.ini";
        /// <summary>
        /// log type
        /// </summary>
        private const string LOG_TYPE = "RestHelper";


        // ====================================================================
        // FIELD
        // ====================================================================

        /// <summary>
        /// 현재 이용중인 거래소
        /// </summary>
        protected EExchange _exchange;
        /// <summary>
        /// 거래소 url base
        /// </summary>
        private readonly string _url;
        /// <summary>
        /// 업비트 api에서 사용하는 AccessKey
        /// </summary>
        private readonly string _accessKey;
        /// <summary>
        /// 업비트 api에서 사용하는 SecretKey
        /// </summary>
        private readonly string _secretKey;
        /// <summary>
        /// 빗썸 api에서 사용하는 api key
        /// </summary>
        private readonly string _apiKey;


        // ====================================================================
        // PROPERTY
        // ====================================================================

        public bool IsDebug { get; protected set; }

        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public RestHelper() : base(Path.Combine(Environment.CurrentDirectory, INI_PATH))
        {
            string doc = MethodBase.GetCurrentMethod().Name;
            const string iniDataErrMsg = "[{0}]섹션의 [{1}]의 값을 찾을 수 없거나 공백입니다.";

            // 각 거래서 URL 베이스 설정
            string urlSection = "URL";
            string upbitBaseUrl = GetIniData(urlSection, nameof(upbitBaseUrl));
            string bithumbBaseUrl = GetIniData(urlSection, nameof(bithumbBaseUrl));

            if (string.IsNullOrEmpty(upbitBaseUrl))
                throw new IniDataException(string.Format(iniDataErrMsg, urlSection, upbitBaseUrl));

            if (string.IsNullOrEmpty(bithumbBaseUrl))
                throw new IniDataException(string.Format(iniDataErrMsg, urlSection, bithumbBaseUrl));

            _url = _exchange switch
            {
                EExchange.Upbit   => upbitBaseUrl,
                EExchange.Bithumb => bithumbBaseUrl,
                _ => throw new ArgumentOutOfRangeException(nameof(_exchange))
            };

            // api 사용에 필요한 key 설정
            string keySection = "KEY";
            string accessKey = GetIniData(keySection, nameof(accessKey));
            string secretKey = GetIniData(keySection, nameof(secretKey));
            string apiKey    = GetIniData(keySection, nameof(apiKey));

            if (string.IsNullOrEmpty(accessKey))
                throw new IniDataException(string.Format(iniDataErrMsg, keySection, accessKey));
            _accessKey = accessKey;

            if (string.IsNullOrEmpty(secretKey))
                throw new IniDataException(string.Format(iniDataErrMsg, keySection, secretKey));
            _secretKey = secretKey;

            if (string.IsNullOrEmpty(apiKey))
                throw new IniDataException(string.Format(iniDataErrMsg, keySection, apiKey));
            _apiKey = apiKey;
        }


        // ====================================================================
        // METHOD
        // ====================================================================

        /// <summary>
        /// RESTful 요청한다.
        /// </summary>
        /// <param name="path">base url을 제외한 이용할 api의 url</param>
        /// <param name="method">RESTful Method</param>
        /// <param name="params">api parameter</param>
        /// <returns>요청에 응답한 json (string)</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool TryRequest(out string content, string path, ERestMethod method, Dictionary<string, string>? @params = null)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            RestSharp.Method restMethod = method switch
            {
                ERestMethod.Get     => RestSharp.Method.Get,
                ERestMethod.Post    => RestSharp.Method.Post,
                ERestMethod.Put     => RestSharp.Method.Put,
                ERestMethod.Delete  => RestSharp.Method.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(method), $"This is not a supported rest method. {method}"),
            };

            StringBuilder sb = new StringBuilder();
            sb.Append(path);

            if (@params != null)
            {
                sb.Append('?');
                sb.Append(GetQueryString(@params));
            }

            var client = new RestClient(_url);
            var request = new RestRequest(sb.ToString(), restMethod);
            request.AddHeader("Content-Type", "application/json");

            if (_exchange == EExchange.Upbit)
            {
                string token = @params != null ? JWTForUpbit(GetQueryString(@params)) : JWTForUpbit();
                request.AddHeader("Authorization", token);
            }
            
            RestResponse response = client.Execute(request);

            content = !string.IsNullOrEmpty(response.Content) ? response.Content : string.Empty;

            return response.IsSuccessful;
        }

        /// <summary>
        /// rest 쿼리를 만들어 반환한다.
        /// </summary>
        /// <param name="params">쿼리 변수</param>
        /// <returns>rest 쿼리</returns>
        private string GetQueryString(Dictionary<string, string> @params)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in @params)
            {
                sb.Append(pair.Key)
                .Append('=')
                .Append(pair.Value)
                .Append('&');
            }

            if (sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        /// <summary>
        /// 업비트 요청시 필요한 JWT 키를 만들어 반환한다.
        /// </summary>
        /// <param name="queryString">쿼리</param>
        /// <returns>JWT</returns>
        private string JWTForUpbit(string? queryString = null)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            TimeSpan diff = DateTime.Now - new DateTime(1970, 01, 01);
            long nonce = Convert.ToInt64(diff.TotalMilliseconds);

            JwtPayload payload;
            if (!string.IsNullOrEmpty(queryString))
            {
                byte[] queryHashByteArray = SHA512.HashData(Encoding.UTF8.GetBytes(queryString));
                string queryHash = BitConverter.ToString(queryHashByteArray).Replace("-", "").ToLower();

                payload = new JwtPayload()
                {
                    { "access_key"      , _accessKey },
                    { "nonce"           , nonce },
                    { "query_hash"      , queryHash },
                    { "query_hash_alg"  , "SHA512" },
                };
            }
            else
            {
                payload = new JwtPayload()
                {
                    { "access_key"  , _accessKey },
                    { "nonce"       , nonce },
                };
            }

            byte[] keyBytes = Encoding.Default.GetBytes(_secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, "HS256");
            var header      = new JwtHeader(credentials);
            var secToken    = new JwtSecurityToken(header, payload);
            var jwtToken    = new JwtSecurityTokenHandler().WriteToken(secToken);

            return $"Bearer {jwtToken}";
        }
    }
}
