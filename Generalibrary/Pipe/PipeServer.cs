using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.07
     *  
     *  < 목적 >
     *  - 파이프 서버 클래스
     *  
     *  < TODO >
     *  
     *  < History >
     *  2024.07.07 @yoon
     *  - 최초 작성
     *  2025.05.01 @yoon
     *  - PipeServer 생성 구조 변경
     *    - 기존: 서버를 생성하기 전 클래스에서 프로세스를 생성
     *    - 변경: 프로세스 생성은 더 이상 해당 클래스에서 생성하지 않음.
     *            생성자 인수로 클라이언트 프로세스 객체를 받음.
     *  ===========================================================================
     */

    public class PipeServer : PipeBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "PipeServer";


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 파이프 서버 스트림
        /// </summary>
        private NamedPipeServerStream _server;


        // ====================================================================
        // PROPERTIES
        // ====================================================================
        
        /// <summary>
        /// 파이프 연결 여부
        /// </summary>
        public override bool IsConnected => _server != null && _server.IsConnected;


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        /// <summary>
        /// 서버 파이프 생성자
        /// </summary>
        /// <param name="pipeName">파이프 이름</param>
        /// <param name="filePath">클라이언트 프로그램 경로</param>
        /// <param name="pipeDirection">파이프 방향 (In,Out,InOut)</param>
        [Obsolete("@yoon 구버전으로 더이상 사용되지 않음.")]
        public PipeServer(string pipeName, string filePath, PipeDirection pipeDirection) : base(pipeName)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            Process client = new Process();
            client.StartInfo.FileName = filePath;
            client.StartInfo.ArgumentList.Add(PIPE_NAME);
            client.StartInfo.UseShellExecute = true;
            client.StartInfo.CreateNoWindow = false;
            
            _server = new NamedPipeServerStream(PIPE_NAME, pipeDirection);
            client.Start();
            _server.WaitForConnection();
            
            _streamString = new StreamString(_server);
        }

        /// <summary>
        /// 서버 파이프 생성자
        /// </summary>
        /// <param name="pipeName">파이프 이름</param>
        /// <param name="client">클라이언트 프로세스</param>
        /// <param name="pipeDirection">파이프 방향 (In, Out, InOut)</param>
        /// <exception cref="ProcessNotStartedException">클라이언트 프로세스를 시작하지 않았다면 발생</exception>
        public PipeServer(string pipeName, Process client, PipeDirection pipeDirection) : base(pipeName)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            try
            {
                int clientId = client.Id;
                Process.GetProcessById(clientId);
            }
            catch (Exception e) when (e is InvalidOperationException || e is ArgumentException)
            {
                // 프로세스의 ID가 설정되어 있지 않을 때
                // ID로 프로세스를 찾을 수 없을 때
                throw new ProcessNotStartedException($"\"{client.ProcessName}\"프로세스(클라이언트)가 실행되지 않은 채 연결을 시도했습니다.");
            }

            _server       = new NamedPipeServerStream(pipeName, pipeDirection);
            _streamString = new StreamString(_server);
        }
    }
}
