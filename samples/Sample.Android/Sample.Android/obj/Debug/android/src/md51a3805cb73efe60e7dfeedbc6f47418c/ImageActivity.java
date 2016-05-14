package md51a3805cb73efe60e7dfeedbc6f47418c;


public class ImageActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("Sample.Android.ImageActivity, ZXingSampleAndroid, Version=2.0.4.17, Culture=neutral, PublicKeyToken=null", ImageActivity.class, __md_methods);
	}


	public ImageActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ImageActivity.class)
			mono.android.TypeManager.Activate ("Sample.Android.ImageActivity, ZXingSampleAndroid, Version=2.0.4.17, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
