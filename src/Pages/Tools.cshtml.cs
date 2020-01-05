using Microsoft.AspNetCore.Mvc.RazorPages;

public class ToolsModel : PageModel
{
  public string message { get; private set; } = "";

  public void OnGet(string action)
  {
    if(action == "gc") {
      System.GC.Collect();
      message = "Garbage collection was run";
    }

    if(action == "mem") {
      double [] stringArray = new double[800 * 1024 * 1000];
      message = "Allocated array with space for "+(800 * 1024 * 1000)+" doubles";
    }      

    if(action == "exception") {
      throw new System.InvalidOperationException("Cheese not found");
    }         
  }
}