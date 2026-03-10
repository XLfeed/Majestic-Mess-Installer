using System.Text;
using Engine;

public class EnemyMarkerTest : Entity
{
    public override void OnInit()
    {
        var markers = Entity.FindScripts<EnemyMarker>();
        if (markers == null || markers.Count == 0)
        {
            Debug.Log("[EnemyMarkerTest] No EnemyMarker scripts found in the scene.");
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("[EnemyMarkerTest] Found markers on entities: ");

        for (int i = 0; i < markers.Count; ++i)
        {
            builder.Append(markers[i].Name);
            if (i < markers.Count - 1)
                builder.Append(", ");
        }

        Debug.Log(builder.ToString());
    }
}
