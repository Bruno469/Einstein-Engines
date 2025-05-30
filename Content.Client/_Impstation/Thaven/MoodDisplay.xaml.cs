using Content.Client.Message;
using Content.Shared._Impstation.Thaven;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Impstation.Thaven;

[GenerateTypedNameReferences]
public sealed partial class MoodDisplay : Control
{
    private string GetSharedString()
    {
        return $"[italic][font size=10][color=gray]{Loc.GetString("moods-ui-shared-mood")}[/color][/font][/italic]";
    }

    public MoodDisplay(ThavenMood mood, bool shared)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        if (shared)
            MoodNameLabel.SetMarkup($"{mood.GetLocName()} {GetSharedString()}");
        else
            MoodNameLabel.SetMarkup(mood.GetLocName());
        MoodDescLabel.SetMarkup(mood.GetLocDesc());
    }
}
