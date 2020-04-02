using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet
{
    public enum ItemModifyMode
    {
        ModeUpdate = 0,
        ModeInsert = 1, 
        ModeUpsert = 2, 
        ModeDelete = 3
    }
}
