public class SC_Ruin : SC_Construction {

    protected override void Start () {

        base.Start();

        transform.parent = SC_UI_Manager.Instance.ruinsT;

    }

}
