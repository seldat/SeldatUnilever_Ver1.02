﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeldatUnilever_Ver1._02.DTO
{
    public class userModel : NotifyUIBase//INotifyPropertyChanged
    {
        private int pCreUsrId;
        private string pCreDt;
        private int pUpdUsrId;
        private string pUpdDt;

        public int creUsrId { get => pCreUsrId; set => pCreUsrId = value; }
        public string creDt { get => pCreDt; set => pCreDt = value; }
        public int updUsrId { get => pUpdUsrId; set => pUpdUsrId = value; }
        public string updDt { get => pUpdDt; set => pUpdDt = value; }
    }
}
