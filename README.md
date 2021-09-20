# Bug Tracker

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![JavaScript](https://img.shields.io/badge/javascript-%23323330.svg?style=for-the-badge&logo=javascript&logoColor=%23F7DF1E)
![HTML5](https://img.shields.io/badge/html5-%23E34F26.svg?style=for-the-badge&logo=html5&logoColor=white)
![CSS3](https://img.shields.io/badge/css3-%231572B6.svg?style=for-the-badge&logo=css3&logoColor=white)
![jQuery](https://img.shields.io/badge/jquery%20-%230769AD.svg?&style=for-the-badge&logo=jquery&logoColor=white)
![AWS](https://img.shields.io/badge/AWS%20-%23FF9900.svg?&style=for-the-badge&logo=amazon-aws&logoColor=white)

By Marco Stari

## Technologies Used

- C#
- Asp.Net MVC
- HTML
- CSS
- JavaScript
- jQuery
- Amazon Web Services (Elastic Beanstalk, EC2, RDS, S3, Route53 )

## How to use it

You can try it out by navigating to https://marcostari-bugtracker.ml/ and registering with a new account or using the guest account.

## Description

For software development companies using a bug tracker is an everyday task. Bug tracking is the process of tracking bugs or errors in issues or tickets. Large systems can have hundreds or thousands of defects. Each one needs to be evaluated, monitored, and prioritized for debugging. In some cases, bugs may need to be tracked over a long period of time.

`Bug Tracker` is a web app developed in C# using the framework ASP.NET MVC that can be used by enterprise companies as well as small development teams.

The user can create new projects and/or partecipate in projects created by others and, depending on the role that he assumes, he can:

- Manage user permissions
- Add/remove user to a project
- Create/edit/delete tickets
- Search/sort/filter tickets
- Add comments to tickets
- Attach files/images to tickets
- View the history of changes (or Timeline) for every ticket

NB: Every user can only see the projects in which he is involved (unless he is an admin).

## Project Roles

A user can be added to a project with one of these 4 roles

- Project Manager:

  - Are allowed to create new projects.
  - Are allowed to edit and delete projects created by them.
  - Are allowed to manage responsibility and access to projects created by them.

- Reporter:

  - Are allowed to create, edit and delete tickets
  - Are allowed to add comments on tickets
  - Are allowed to attach files on tickets
  - Are not allowed to change the status of the ticket

- Developer:

  - Are allowed to update the ticket status from To Do to In Progress and from In Progress to In Review
  - Are allowed to attach and delete files to tickets
  - Are allowed to add comments on tickets
  - Are not allowed to create or delete tickets
  - Are not allowed to update any ticket property other than the status
  - Are not allowed to close bug

- Reviewer:

  - Are allowed to update the ticket status from In Review to Done and from In Review to In Progress
  - Are allowed to add comments on tickets
  - Are allowed to attach files to tickets
  - Are not allowed to create and delete tickets
  - Are not allowed to update any ticket property other than the status

## Features

- Share your data with other users through a qr code.
- Decide what information a user is allowed to see.
- Keep your contacts' data always in sync.
- If you want to see the data of the contacts that are locally stored on your phone, you are able to do so.
- Press and hold on a contact information to perform an action (depending on the type of the information you can: call a number, send an email, copy the text, open google maps ecc...)
