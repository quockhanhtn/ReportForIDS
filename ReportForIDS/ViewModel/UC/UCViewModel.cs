namespace ReportForIDS.ViewModel
{
   public abstract class UCViewModel : BaseViewModel
   {
      public bool Done { get; set; } = false;
      public virtual void ReLoad() { }
      public virtual bool LoadPreviosData() { return false; }
   }
}
