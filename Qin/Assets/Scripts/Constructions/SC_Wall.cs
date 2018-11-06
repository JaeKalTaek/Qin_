public class SC_Wall : SC_Bastion {

    protected override void Start() {

        base.Start();

        transform.parent = uiManager.wallsT;

    }

}
