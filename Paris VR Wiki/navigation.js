function ShowAboutWiki()
{
	HideAllContent();
	document.getElementById("mainPage").style.display = "block";
}
			
function ShowTeamWiki()
{
	HideAllContent();
	document.getElementById("teamPage").style.display = "block";
}
			
function ShowToolkitWiki()
{
	HideAllContent();
	document.getElementById("toolkitPage").style.display = "block";
}
			
function ShowVRWiki()
{
	HideAllContent();
	document.getElementById("vrAppPage").style.display = "block";
}
			
function HideAllContent()
{
	document.getElementById("mainPage").style.display = "none";
	document.getElementById("teamPage").style.display = "none";
	document.getElementById("toolkitPage").style.display = "none";
	document.getElementById("vrAppPage").style.display = "none";
}