import QtQuick 2.12
import QtQuick.Window 2.12
import QtQuick.Controls 2.12
import QtQuick.Controls.Material 2.12
import QtQuick.Layouts 1.1
import SS14Launcher 1.0

ApplicationWindow {
    visible: true
    title: qsTr("Space Station 14 Launcher")
    width: 500
    height: 300

    minimumWidth: pane.implicitWidth
    minimumHeight: pane.implicitHeight

    Material.theme: Material.Dark
    Material.primary: Material.Indigo
    Material.background: "#20202a"
    Material.accent: "#A88B5E"
    Material.foreground: "#e0e0e0"

    FontLoader { source: "../fonts/Animal Silence.otf" }

    Launcher {
        id: launcher
        onStatusChanged: {
            switch (launcher.status) {
                case 0:
                    progressLabel.text = qsTr("Checking for launcher update..");
                    break;
                case 1:
                    progressLabel.text = qsTr("Checking for client update..");
                    break;
                case 2:
                    progressLabel.text = qsTr("Downloading update..");
                    break;

                case 3:
                    progressLabel.text = qsTr("Extracting update..");
                    break;

                case 4:
                    progressLabel.text = qsTr("Ready!");
                    break;

                case 5:
                    progressLabel.text = qsTr("Error connecting to builds.spacestation14.io!")
                    break;

                case 6:
                    progressLabel.text = qsTr("This launcher is out of date. <a href=\"https://spacestation14.io/about/nightlies/\">Download update.</a>")
                    break;
            };
        }
    }

    Pane {
        id: pane
        anchors.fill: parent

        ColumnLayout {
            id: mainColumn

            anchors.fill: parent
            anchors.leftMargin: 5
            anchors.rightMargin: 5
            Label {
                Layout.alignment: Qt.AlignHCenter
                text: qsTr("Space Station 14")
                font.family: "Animal Silence"
                font.pointSize: 30
            }

            Label {
                text: qsTr("<ul>
                <li><a href=\"https://spacestation14.io\">website</a></li>
                </ul>")
                font.pointSize: 14
                textFormat: Text.RichText
                onLinkActivated: Qt.openUrlExternally(link)
                Layout.fillHeight: true
            }

            RowLayout {
                Layout.fillWidth: true

                Label {
                    id: progressLabel
                    font.pointSize: 12
                    Layout.fillWidth: launcher.status === 5 || launcher.status === 6
                    onLinkActivated: Qt.openUrlExternally(link)
                    textFormat: Text.StyledText
                }

                ProgressBar {
                    id: progressBar
                    Layout.fillWidth: true
                    indeterminate: launcher.progress < 0
                    value: launcher.progress
                    visible: launcher.status !== 5 && launcher.status !== 6
                }

                Button {
                    id: launchButton
                    text: qsTr("Launch")
                    Layout.alignment: Qt.AlignRight
                    enabled: launcher.status === 4
                    onClicked: {
                        launcher.launchClient();
                    }
                }
            }
        }
    }

    Component.onCompleted: function() {
        launcher.startUpdate()
    }
}