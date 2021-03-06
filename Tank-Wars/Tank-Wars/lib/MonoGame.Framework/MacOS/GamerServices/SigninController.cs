using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace Microsoft.Xna.Framework.GamerServices
{
	public partial class SigninController : MonoMac.AppKit.NSWindowController
	{
		
		NSApplication NSApp = NSApplication.SharedApplication;	
		
		
		internal List<MonoGameLocalGamerProfile> gamerList;
		internal NSTableView tableView;
		
		#region Constructors

		// Called when created from unmanaged code
		public SigninController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public SigninController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

//		// Call to load from the XIB/NIB file
//		public SigninController () : base("TableEdit")
//		{
//			Initialize ();
//		}
		
		public SigninController()
		{
			Initialize();
		}
		
		NSWindow window = null;
		internal NSButton selectButton;
		
		// Shared initialization code
		void Initialize ()
		{
			//window = new NSWindow(new RectangleF(0,0, 470, 250), NSWindowStyle.Titled | NSWindowStyle.Closable, NSBackingStore.Buffered, false);
			window = new NSWindow(new RectangleF(0,0, 470, 250), NSWindowStyle.Titled, NSBackingStore.Buffered, false);
			window.HasShadow = true;
			NSView content = window.ContentView;
			window.WindowController = this;
			window.Title = "Sign In";
			NSTextField signInLabel = new NSTextField(new RectangleF(17, 190, 109, 17));
			signInLabel.StringValue = "Sign In:";
			signInLabel.Editable = false;
			signInLabel.Bordered = false;
			signInLabel.BackgroundColor = NSColor.Control;
			
			content.AddSubview(signInLabel);
			
			// Create our select button
			selectButton = new NSButton(new RectangleF(358,12,96,32));
			selectButton.Title = "Select";
			selectButton.SetButtonType(NSButtonType.MomentaryPushIn);
			selectButton.BezelStyle = NSBezelStyle.Rounded;
			
			selectButton.Activated += delegate {
				
				profileSelected();
			};
			
			selectButton.Enabled = false;
			
			content.AddSubview(selectButton);
			
			// Setup our table view
			NSScrollView tableContainer = new NSScrollView(new RectangleF(20,60,428, 123));
			tableContainer.BorderType = NSBorderType.BezelBorder;
			tableContainer.AutohidesScrollers = true;
			tableContainer.HasVerticalScroller = true;
			
			tableView = new NSTableView(new RectangleF(0,0,420, 123));
			tableView.UsesAlternatingRowBackgroundColors = true;
			
			NSTableColumn colGamerTag = new NSTableColumn(new NSString("Gamer"));
			tableView.AddColumn(colGamerTag);
			
			colGamerTag.Width = 420;
			colGamerTag.HeaderCell.Title = "Gamer Profile";
			tableContainer.DocumentView = tableView;
			
			content.AddSubview(tableContainer);
			
			// Create our add button
			NSButton addButton = new NSButton(new RectangleF(20,27,25,25));
			//Console.WriteLine(NSImage.AddTemplate);
			addButton.Image = NSImage.ImageNamed("NSAddTemplate");
			addButton.SetButtonType(NSButtonType.MomentaryPushIn);
			addButton.BezelStyle = NSBezelStyle.SmallSquare;
			
			addButton.Activated += delegate {
				addLocalPlayer();
			};
			content.AddSubview(addButton);
			
			// Create our remove button
			NSButton removeButton = new NSButton(new RectangleF(44,27,25,25));
			removeButton.Image = NSImage.ImageNamed("NSRemoveTemplate");
			removeButton.SetButtonType(NSButtonType.MomentaryPushIn);
			removeButton.BezelStyle = NSBezelStyle.SmallSquare;
			
			removeButton.Activated += delegate {
				removeLocalPlayer();
			};
			content.AddSubview(removeButton);			
			
			gamerList = MonoGameGamerServicesHelper.DeserializeProfiles();
			
//			for (int x= 1; x< 25; x++) {
//				gamerList.Add("Player " + x);
//			}
			tableView.DataSource = new GamersDataSource(this);
			tableView.Delegate = new GamersTableDelegate(this);
		}

		#endregion
		

		public new NSWindow Window {
			get { return window; }
		}
		
		void profileSelected () 
		{
			if (tableView.SelectedRowCount > 0) {
				var rowSelected = tableView.SelectedRow;
				SignedInGamer sig = new SignedInGamer();
				sig.DisplayName = gamerList[rowSelected].DisplayName;
				sig.Gamertag = gamerList[rowSelected].Gamertag;
				sig.InternalIdentifier = gamerList[rowSelected].PlayerInternalIdentifier;
				
				Gamer.SignedInGamers.Add(sig);
			}
			MonoGameGamerServicesHelper.SerializeProfiles(gamerList);
			NSApp.StopModal();
		}
		
		void removeLocalPlayer () 
		{
			Console.WriteLine("Remove local");
			var rowToRemove = tableView.SelectedRow;
			if (rowToRemove < 0 || rowToRemove > gamerList.Count -1)
				return;
			gamerList.RemoveAt(rowToRemove);
			tableView.ReloadData();
		}
		
		void addLocalPlayer () 
		{
			MonoGameLocalGamerProfile prof = new MonoGameLocalGamerProfile();
			prof.Gamertag = "New Player";
			prof.DisplayName = prof.Gamertag;

			gamerList.Add(prof);
			
			tableView.ReloadData();
			int newPlayerRow = gamerList.Count - 1;
			
			tableView.ScrollRowToVisible(newPlayerRow);
			tableView.EditColumn(0,newPlayerRow, new NSEvent(), true);
			
		}		
	}
	
	public class GamersDataSource : NSTableViewDataSource
	{
		
		SigninController controller;
		
		public GamersDataSource (SigninController controller) 
		{
			this.controller = controller;
		}
		
		public override int GetRowCount (NSTableView tableView)
		{
			return controller.gamerList.Count;
		}
		
		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			return new NSString(controller.gamerList[row].Gamertag);
		}
		
		public override void SetObjectValue (NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int row)
		{
			var proposedValue = theObject.ToString();
			if (proposedValue.Trim().Length > 0) {
				controller.gamerList[row].Gamertag = theObject.ToString();
				controller.gamerList[row].DisplayName = theObject.ToString();
			}
		}
		
	}
	
	public class GamersTableDelegate : NSTableViewDelegate
	{
		SigninController controller;
		
		public GamersTableDelegate (SigninController controller) 
		{
			this.controller = controller;
		}
		
		public override bool ShouldSelectRow (NSTableView tableView, int row)
		{
			var profile = controller.gamerList[row];
			foreach (var gamer in Gamer.SignedInGamers) {
				if (profile.PlayerInternalIdentifier == gamer.InternalIdentifier)
					return false;
			}
			return true;
		}
		
		public override void SelectionDidChange (NSNotification notification)
		{
			if (controller.tableView.SelectedRowCount > 0)
				controller.selectButton.Enabled = true;
			else
				controller.selectButton.Enabled = false;
		}
		
	}
}

