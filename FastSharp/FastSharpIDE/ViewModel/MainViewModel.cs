// Copyright 2014 Toni Petrina
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using GalaSoft.MvvmLight.Command;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using System;
using System.Threading.Tasks;

namespace FastSharpIDE.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Roslyn interaction
        private ScriptEngine _engine;
        private Session _session;
        #endregion

        #region Bindable properties
        public string Text
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public ExecutionResultViewModel ExecutionResult
        {
            get { return Get<ExecutionResultViewModel>(); }
            set { Set(value); }
        }

        public StatusViewModel Status
        {
            get { return Get<StatusViewModel>(); }
            set { Set(value); }
        }

        public bool IsExecuting
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }
        #endregion

        public RelayCommand<string> ExecuteCommand { get; set; }

        public MainViewModel()
        {
            ExecuteCommand = new RelayCommand<string>(Execute);
        }

        private async void Execute(string code)
        {
            await ExecuteInternalAsync(code);
        }

        private async Task ExecuteInternalAsync(string code)
        {
            if (IsExecuting)
                return;
            IsExecuting = true;

            ExecutionResult = new ExecutionResultViewModel();

            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    ExecutionResult = new ExecutionResultViewModel
                    {
                        Message = "Nothing to execute",
                        Type = ExecutionResultType.Warning
                    };
                    Status.SetReady();
                    return;
                }

                Status.SetInfo("Executing...");

                var o = await Task.Run(() => _session.Execute(code));

                Status.SetInfo("Executed");
                var message = o == null ? "** no results from the execution **" : o.ToString();

                ExecutionResult = new ExecutionResultViewModel
                {
                    Message = message,
                    Type = ExecutionResultType.Success
                };
            }
            catch (Exception e)
            {
                ExecutionResult = new ExecutionResultViewModel
                {
                    Message = e.ToString(),
                    Type = ExecutionResultType.Error
                };
                Status.SetStatus("Failed", StatusType.Error);
            }

            IsExecuting = false;
        }

        public void Load()
        {
            _engine = new ScriptEngine();
            _session = _engine.CreateSession();
            Status = new StatusViewModel();
        }
    }
}