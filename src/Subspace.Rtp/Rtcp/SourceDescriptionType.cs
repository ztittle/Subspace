namespace Subspace.Rtp.Rtcp
{
    public enum SourceDescriptionType : byte
    {
        /// <summary>
        /// item type of zero to denote the end of the list.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5
        /// </summary>
        End = 0,
        /// <summary>
        /// CNAME: Canonical End-Point Identifier SDES Item
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.1
        /// </summary>
        CName = 1,

        /// <summary>
        /// NAME: User Name SDES Item
        ///
        /// This is the real name used to describe the source, e.g., "John Doe,
        /// Bit Recycler".  It may be in any form desired by the user.  For
        /// applications such as conferencing, this form of name may be the most
        /// desirable for display in participant lists, and therefore might be
        /// sent most frequently of those items other than CNAME.  Profiles MAY
        /// establish such priorities.  The NAME value is expected to remain
        /// constant at least for the duration of a session.  It SHOULD NOT be
        /// relied upon to be unique among all participants in the session.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.2
        /// </summary>
        Name = 2,

        /// <summary>
        /// EMAIL: Electronic Mail Address SDES Item
        ///
        /// The email address is formatted according to RFC 2822 [9], for
        /// example, "John.Doe@example.com".  The EMAIL value is expected to
        /// remain constant for the duration of a session.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.2
        /// </summary>
        Email = 3,

        /// <summary>
        /// PHONE: Phone Number SDES Item
        /// 
        /// The phone number SHOULD be formatted with the plus sign replacing the
        /// international access code.  For example, "+1 908 555 1212" for a
        /// number in the United States.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.4
        /// </summary>
        Phone = 4,

        /// <summary>
        /// LOC: Geographic User Location SDES Item
        /// 
        /// Depending on the application, different degrees of detail are
        /// appropriate for this item.  For conference applications, a string
        /// like "Murray Hill, New Jersey" may be sufficient, while, for an
        /// active badge system, strings like "Room 2A244, AT&amp;T BL MH" might be
        /// appropriate.  The degree of detail is left to the implementation
        /// and/or user, but format and content MAY be prescribed by a profile.
        /// The LOC value is expected to remain constant for the duration of a
        /// session, except for mobile hosts.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.5
        /// </summary>
        Location = 5,

        /// <summary>
        /// TOOL: Application or Tool Name SDES Item
        ///
        /// A string giving the name and possibly version of the application
        /// generating the stream, e.g., "videotool 1.2".  This information may
        /// be useful for debugging purposes and is similar to the Mailer or
        /// Mail-System-Version SMTP headers.  The TOOL value is expected to
        /// remain constant for the duration of the session.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.5.6
        /// </summary>
        Tool = 6,

        /// <summary>
        /// NOTE: Notice/Status SDES Item
        ///
        /// The NOTE
        /// item is intended for transient messages describing the current state
        /// of the source, e.g., "on the phone, can't talk". 
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.7
        /// </summary>
        Note = 7,

        /// <summary>
        /// PRIV: Private Extensions SDES Item
        ///
        /// This item is used to define experimental or application-specific SDES
        /// extensions.  The item contains a prefix consisting of a length-string
        /// pair, followed by the value string filling the remainder of the item
        /// and carrying the desired information.  The prefix length field is 8
        /// bits long.  The prefix string is a name chosen by the person defining
        /// the PRIV item to be unique with respect to other PRIV items this
        /// application might receive.  The application creator might choose to
        /// use the application name plus an additional subtype identification if
        /// needed.  Alternatively, it is RECOMMENDED that others choose a name
        /// based on the entity they represent, then coordinate the use of the
        /// name within that entity.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.5.8
        /// </summary>
        PrivateExtension = 8
    }
}