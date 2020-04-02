using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Indexes : ItemsOf<Index>
    {

    }
}
