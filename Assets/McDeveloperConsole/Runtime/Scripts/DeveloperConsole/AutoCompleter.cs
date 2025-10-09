using System;
using System.Collections.Generic;
using System.Text;
using Machamy.DeveloperConsole.Commands;

namespace Machamy.DeveloperConsole
{
    /// <summary>
    /// (eng) Provides auto-completion suggestions for console input.<br/>
    /// (kor) 콘솔 입력에 대한 자동 완성 제안을 제공합니다.
    /// </summary>
    public class AutoCompleter 
    {
        private string _chachedInput = "I_AM_GROOT";
        private List<string> _cachedSuggestions = new List<string>();
        private int _currentIndex = -1;
        
        private int _completTarget = -1; // 0: command part, 1...: arg part
        
        /// <summary>
        /// (eng) The last input string that was used to generate suggestions.<br/>
        /// (kor) 제안을 생성하는 데 사용된 마지막 입력 문자열입니다.
        /// </summary>
        public string ChachedInput => _chachedInput;
        /// <summary>
        /// (eng) The currently selected suggestion.<br/>
        /// (kor) 현재 선택된 제안입니다.
        /// </summary>
        public string CurrentSuggestion => GetCurrentFullSuggestion();
        /// <summary>
        /// (eng) The list of current suggestions.<br/>
        /// (kor) 현재 제안 목록입니다.
        /// </summary>
        public IReadOnlyList<string> CurrentSuggestions => _cachedSuggestions.AsReadOnly();
        public int CurrentIndex => _currentIndex;
        public int CurrentTarget => _completTarget;
        public int SuggestionCount => _cachedSuggestions.Count;
        public bool HasSuggestions => _cachedSuggestions.Count > 0;
        
        public AutoCompleter()
        {
            
        }
        
        /// <summary>
        /// (eng) Clears the cached input and suggestions.<br/>
        /// (kor) 캐시된 입력과 제안을 지웁니다.
        /// </summary>
        public void Clear()
        {
            _chachedInput = "";
            _cachedSuggestions.Clear();
            _currentIndex = -1;
        }
        
        /// <summary>
        /// (eng) Updates the auto-completion suggestions based on the new input string.<br/>
        /// Resets the current selection index.<br/>
        /// (kor) 새로운 입력 문자열을 기반으로 자동 완성 제안을 업데이트합니다.<br/>
        /// 현재 선택 인덱스를 재설정합니다.
        /// </summary>
        /// <param name="input"></param>
        public void TextInputChanged(string input)
        {
            _chachedInput = input;
            GetSuggestions(_chachedInput, ref _cachedSuggestions);
            _currentIndex = -1;
        }
        
        /// <summary>
        /// (eng) Gets the currently selected suggestion.<br/>
        /// Returns null if no suggestion is selected.<br/>
        /// (kor) 현재 선택된 제안을 가져옵니다.<br/>
        /// 선택된 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentSuggestion()
        {
            if (_currentIndex < 0 || _currentIndex >= _cachedSuggestions.Count)
                return null;
            return _cachedSuggestions[_currentIndex];
        }

        /// <summary>
        /// (eng) Moves to the next suggestion and returns it.<br/>
        /// Wraps around to the first suggestion if at the end.<br/>
        /// Returns null if there are no suggestions.<br/>
        /// (kor) 다음 제안으로 이동하고 그것을 반환합니다.<br/>
        /// 끝에 도달하면 첫 번째 제안으로 감싸집니다.<br/>
        /// 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetNextSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex + 1) % _cachedSuggestions.Count;
            return _cachedSuggestions[_currentIndex];
        }
        /// <summary>
        /// (eng) Moves to the previous suggestion and returns it.<br/>
        /// Wraps around to the last suggestion if at the beginning.<br/>
        /// Returns null if there are no suggestions.<br/>
        /// (kor) 이전 제안으로 이동하고 그것을 반환합니다.<br/>
        /// 시작 부분에 도달하면 마지막 제안으로 감싸집니다.<br/>
        /// 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetPreviousSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex - 1 + _cachedSuggestions.Count) % _cachedSuggestions.Count;
            return _cachedSuggestions[_currentIndex];
        }
        /// <summary>
        /// (eng) Gets the currently selected suggestion with the full input context.<br/>
        /// For example, if the input is "setHealth 100" and the suggestion is "200", it returns "setHealth 200".<br/>
        /// Returns null if no suggestion is selected.<br/>
        /// (kor) 전체 입력 컨텍스트와 함께 현재 선택된 제안을 가져옵니다.<br/>
        /// 예를 들어, 입력이 "setHealth 100"이고 제안이 "200"인 경우 "setHealth 200"을 반환합니다.<br/>
        /// 선택된 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentFullSuggestion()
        {
            if (_currentIndex < 0 || _currentIndex >= _cachedSuggestions.Count)
                return null;
            return GetFullSuggestion(_currentIndex);
        }
        /// <summary>
        /// (eng) Moves to the next suggestion and returns it with the full input context.<br/>
        /// Wraps around to the first suggestion if at the end.<br/>
        /// For example, if the input is "setHealth 100" and the suggestion is "200", it returns "setHealth 200".<br/>
        /// Returns null if there are no suggestions.<br/>
        /// (kor) 다음 제안으로 이동하고 전체 입력 컨텍스트와 함께 그것을 반환합니다.<br/>
        /// 끝에 도달하면 첫 번째 제안으로 감싸집니다.<br/>
        /// 예를 들어, 입력이 "setHealth 100"이고 제안이 "200"인 경우 "setHealth 200"을 반환합니다.<br/>
        /// 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetNextFullSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex + 1) % _cachedSuggestions.Count;
            return GetFullSuggestion(_currentIndex);
        }
        /// <summary>
        /// (eng) Moves to the previous suggestion and returns it with the full input context.<br/>
        /// Wraps around to the last suggestion if at the beginning.<br/>
        /// For example, if the input is "setHealth 100" and the suggestion is "200", it returns "setHealth 200".<br/>
        /// Returns null if there are no suggestions.<br/>
        /// (kor) 이전 제안으로 이동하고 전체 입력 컨텍스트와 함께 그것을 반환합니다.<br/>
        /// 시작 부분에 도달하면 마지막 제안으로 감싸집니다.<br/>
        /// 예를 들어, 입력이 "setHealth 100"이고 제안이 "200"인 경우 "setHealth 200"을 반환합니다.<br/>
        /// 제안이 없으면 null을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string GetPreviousFullSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex - 1 + _cachedSuggestions.Count) % _cachedSuggestions.Count;
            return GetFullSuggestion(_currentIndex);
        }
        
        /// <summary>
        /// (eng) Combines the suggestion at the given index with the full input context.<br/>
        /// For example, if the input is "setHealth 100" and the suggestion is "200", it returns "setHealth 200".<br/>
        /// Returns null if the index is out of range.<br/>
        /// (kor) 주어진 인덱스의 제안을 전체 입력 컨텍스트와 결합합니다.<br/>
        /// 예를 들어, 입력이 "setHealth 100"이고 제안이 "200"인 경우 "setHealth 200"을 반환합니다.<br/>
        /// 인덱스가 범위를 벗어나면 null을 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetFullSuggestion(int index)
        {
            if (index < 0 || index >= _cachedSuggestions.Count)
                return null;
            if (_completTarget == 0)
            {
                return _cachedSuggestions[index];
            }else if (_completTarget > 0)
            {
                string[] parts = _chachedInput.Split(' ');
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _completTarget; i++)
                {
                    sb.Append(parts[i]);
                    sb.Append(' ');
                }
                sb.Append(_cachedSuggestions[index]);
                return sb.ToString();
            }
            else
            {
                return _cachedSuggestions[index];
            }
        }
        
        /// <summary>
        /// (eng) Sets the current suggestion index.<br/>
        /// Returns true if the index is valid and set, false otherwise.<br/>
        /// (kor) 현재 제안 인덱스를 설정합니다.<br/>
        /// 인덱스가 유효하고 설정되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool SetCurrentIndex(int index)
        {
            if (index < 0 || index >= _cachedSuggestions.Count)
                return false;
            _currentIndex = index;
            return true;
        }

        /// <summary>
        /// (eng) Sets the current suggestion by value.<br/>
        /// Returns true if the suggestion exists and is set, false otherwise.<br/>
        /// (kor) 값으로 현재 제안을 설정합니다.<br/>
        /// 제안이 존재하고 설정되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="suggestion"></param>
        /// <returns></returns>
        public bool SetCurrentSuggestion(string suggestion)
        {
            int index = _cachedSuggestions.IndexOf(suggestion);
            if (index == -1)
                return false;
            _currentIndex = index;
            return true;
        }
        
        /// <summary>
        /// (eng) Gets all current suggestions as a read-only list.<br/>
        /// (kor) 현재 모든 제안을 읽기 전용 목록으로 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<string> GetAllSuggestions()
        {
            return _cachedSuggestions.AsReadOnly();
        }
        
        /// <summary>
        /// (eng) Generates auto-completion suggestions based on the input string.<br/>
        /// The suggestions are returned in the provided list reference.<br/>
        /// (kor) 입력 문자열을 기반으로 자동 완성 제안을 생성합니다.<br/>
        /// 제안은 제공된 목록 참조에 반환됩니다.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="suggestions"></param>
        public void GetSuggestions(string input, ref List<string> suggestions)
        {
            if(suggestions == null)
                suggestions = new List<string>();
            else
                suggestions.Clear();
            // TODO : 배열 복사가 빈번하게 일어남
            string[] parts = input.Split(' ');
            if (parts.Length == 0)
            {
                return;
            }
            string commandPart = parts[0];
            Span<string> argsPart = new Span<string>(parts, 1, parts.Length - 1);
            for (int i = 1; i < parts.Length; i++)
                argsPart[i - 1] = parts[i];
            
            _completTarget = argsPart.Length;

            // 명령어부분
            if (_completTarget == 0)
            {
                foreach (var cmd in CommandLibrary.GetAllCommands())
                {
                    if (cmd.Command.StartsWith(commandPart))
                    {
                        suggestions.Add(cmd.Command);
                    }
                }
                return;
            }
            
            // 인자부분
            IConsoleCommand command;
            if (!CommandLibrary.TryGetCommand(commandPart, out command))
            {
                return;
            }
            command.AutoComplete(argsPart, ref suggestions);

            return;
        }
        
    }
}