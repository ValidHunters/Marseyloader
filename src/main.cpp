#include <QGuiApplication>
#include <QIcon>
#include <QResource>
#include <iostream>

extern "C" {
    void ss14l_setIcon(QGuiApplication* app, const char* path) {
        app->setWindowIcon(QIcon(path));
    }

    void ss14l_registerRcc(const char* path) {
        auto a = QCoreApplication::applicationDirPath();
        std::cout << a.toUtf8().constData() << std::endl;
    }
}