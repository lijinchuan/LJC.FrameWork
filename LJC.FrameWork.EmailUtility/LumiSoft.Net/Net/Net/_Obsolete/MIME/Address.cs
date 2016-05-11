using System;

namespace LJC.FrameWork.Net.Mime
{
	/// <summary>
	/// Rfc 2822 3.4 Address class. This class is base class for MailboxAddress and GroupAddress.
	/// </summary>
    [Obsolete("See LJC.FrameWork.Net.MIME or LJC.FrameWork.Net.Mail namepaces for replacement.")]
	public abstract class Address
	{
		private bool   m_GroupAddress = false;
		private object m_pOwner       = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="groupAddress">Spcified is address is group or mailbox address.</param>
		public Address(bool groupAddress)
		{
			m_GroupAddress = groupAddress;
		}


		#region Properties Implementation

		/// <summary>
		/// Gets if address is group address or mailbox address.
		/// </summary>
		public bool IsGroupAddress
		{
			get{ return m_GroupAddress; }
		}


		/// <summary>
		/// Gets or sets owner of this address.
		/// </summary>
		internal object Owner
		{
			get{ return m_pOwner; }

			set{ m_pOwner = value; }
		}

		#endregion

	}
}
