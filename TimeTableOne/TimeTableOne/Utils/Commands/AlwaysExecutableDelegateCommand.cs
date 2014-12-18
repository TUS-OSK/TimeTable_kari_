﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTableOne.Utils.Commands
{
    public class AlwaysExecutableDelegateCommand : DelegateCommandBase
    {

        public AlwaysExecutableDelegateCommand(Action act)
            : base(act)
        {

        }
        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
