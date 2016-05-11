using System;

namespace LJC.FrameWork.Net.Mime
{
	/// <summary>
	/// Rfc 2183 Content-Disposition.
	/// </summary>
    [Obsolete("See LJC.FrameWork.Net.MIME or LJC.FrameWork.Net.Mail namepaces for replacement.")]
	public enum ContentDisposition_enum
	{
		/// <summary>
		/// Content is attachment.
		/// </summary>
		Attachment = 0,

		/// <summary>
		/// Content is embbed resource.
		/// </summary>
		Inline = 1,

		/// <summary>
		/// Content-Disposition header field isn't available or isn't written to mime message.
		/// </summary>
		NotSpecified = 30,

		/// <summary>
		/// Content is unknown.
		/// </summary>
		Unknown = 40
	}
}
