using System;

using AppKit;
using Foundation;

namespace VtNetCore.Cocoa
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.

            textUri.StringValue = "ssh://10.100.5.100";
            textUsername.StringValue = "admin";
            textPassword.StringValue = "Minions12345";
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        partial void connectClicked(Foundation.NSObject sender)
        {
            Terminal.ConnectTo(textUri.StringValue, textUsername.StringValue, textPassword.StringValue);
        }
    }
}
