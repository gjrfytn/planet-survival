﻿using System.Collections;
using System.Collections.Generic;

public static class EventDictionary
{
    public static readonly Dictionary<string, string> TagDictionary = new Dictionary<string, string>{
		{"damage","Урона получено: "},
		{"heal","Здоровья получено: "},
		{"enemy","На вас напал "}
	};
}
