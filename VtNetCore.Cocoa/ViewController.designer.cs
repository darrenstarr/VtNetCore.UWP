// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace VtNetCore.Cocoa
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		VtNetCore.Cocoa.TerminalView Terminal { get; set; }

		[Outlet]
		AppKit.NSSecureTextField textPassword { get; set; }

		[Outlet]
		AppKit.NSTextFieldCell textUri { get; set; }

		[Outlet]
		AppKit.NSTextField textUsername { get; set; }

		[Action ("connectClicked:")]
		partial void connectClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (Terminal != null) {
				Terminal.Dispose ();
				Terminal = null;
			}

			if (textUri != null) {
				textUri.Dispose ();
				textUri = null;
			}

			if (textUsername != null) {
				textUsername.Dispose ();
				textUsername = null;
			}

			if (textPassword != null) {
				textPassword.Dispose ();
				textPassword = null;
			}
		}
	}
}
