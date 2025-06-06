namespace Generalibrary
{

    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.05
     *  
     *  < 목적 >
     *  시스템에서 사용되는 옵션을 관리하는 클래스
     *  [중요] 무조건 LogManager.Instance 호출보다 먼저 초기화 되어야 한다.
     *  
     *  < TODO >
     *  -
     *  
     *  < History >
     *  2025.06.05 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class SystemInfo : IniHelper
    {
        #region SINGLETON
        private static SystemInfo? _info;
        public static SystemInfo Info
        {
            get
            {
                _info ??= new SystemInfo();
                return _info;
            }
            set
            {
                _info = value;
            }
        }
        #endregion

        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string INI_PATH = "ini\\system.ini";

        private const string LOG_TYPE = "SystemInfo";


        // ====================================================================
        // PROPERTIES
        // ====================================================================

        public StartOption StartOption => _startOption;


        // ====================================================================
        // FIELDS
        // ====================================================================

        private bool _isInitialized = false;

        private StartOption _startOption;

        
        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        private SystemInfo() : base(INI_PATH)
        {

        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// Systeminfo를 생성하고 처음 초기설정을 진행한다
        /// </summary>
        /// <param name="startOption"></param>
        public void Initializer(StartOption startOption)
        {
            if (_isInitialized)
                return;

            _startOption = startOption == null ? 
                           new StartOption() : 
                           startOption;

            _isInitialized = true;
        }
    }
}
