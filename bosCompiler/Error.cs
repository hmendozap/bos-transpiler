﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BosTranspiler
{
    public class Error
    {
        public string Stack { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Exception { get; set; } = "";
        public string Msg { get; set; } = "";
        public string File { get; set; } = "";
    }
}