﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LJC.FrameWork.Net
{
    /// <summary>
	/// Represent the method what will handle Error event.
	/// </summary>
	/// <param name="sender">Delegate caller.</param>
	/// <param name="e">Event data.</param>
	public delegate void ErrorEventHandler(object sender,Error_EventArgs e);
}
