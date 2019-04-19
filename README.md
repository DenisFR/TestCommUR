# TestCommUR
This programm is here to test communication with Universal-Robot's Controller using RTDE Protocol.
https://www.universal-robots.com/how-tos-and-faqs/how-to/ur-how-tos/real-time-data-exchange-rtde-guide-22229/

The URController class is used to manage communication. You can import it on your own project.
See on Form1 how to use it.

## Usage
![TestCommUR](https://raw.githubusercontent.com/DenisFR/TestCommUR/master/TestCommUR/TestCommUR.jpg)

Enter your UR controller IP in text field.
Click on Connect (Protocol version is wrote on right)
Click on Get Ctrl Version to populate list of in/outputs depending the version.

When Controller send message, Received Text is updated.
Clear button erase it.

To send a message, select the warning level with the ComboBox, fill source field and your message then click on Send button.

To receive datas from controller, select them on left list then click on Start Receive button.
You can pause it or change frequency.

To send datas to controller, first enter datas value then check it. When ready, click on Send button.
If data values are malformed they are colored to red.
When controller accept data setup, they are colored in green.

The form could be resized.