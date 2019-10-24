using System;
using System.Collections.Generic;
using CowSpeak;
using System.Diagnostics;

class MainClass {
	public static void Main(string[] args) {
		CowSpeak.CowSpeak.Run("stringThings.COWFILE", false);
		CowSpeak.CowSpeak.Run("calculator.COWFILE", false);
		CowSpeak.CowSpeak.Run("stairsMaker.COWFILE", false);
		CowSpeak.CowSpeak.Run("randomNumberGuessr.COWFILE", false);
	}
}