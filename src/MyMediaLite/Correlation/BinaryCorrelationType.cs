



<!DOCTYPE html>
<html lang="en" class=" is-copy-enabled">
  <head prefix="og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# object: http://ogp.me/ns/object# article: http://ogp.me/ns/article# profile: http://ogp.me/ns/profile#">
    <meta charset='utf-8'>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta http-equiv="Content-Language" content="en">
    <meta name="viewport" content="width=1020">
    
    
    <title>MyMediaLite/BinaryCorrelationType.cs at master · zenogantner/MyMediaLite</title>
    <link rel="search" type="application/opensearchdescription+xml" href="/opensearch.xml" title="GitHub">
    <link rel="fluid-icon" href="https://github.com/fluidicon.png" title="GitHub">
    <link rel="apple-touch-icon" sizes="57x57" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="114x114" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="72x72" href="/apple-touch-icon-144.png">
    <link rel="apple-touch-icon" sizes="144x144" href="/apple-touch-icon-144.png">
    <meta property="fb:app_id" content="1401488693436528">

      <meta content="@github" name="twitter:site" /><meta content="summary" name="twitter:card" /><meta content="zenogantner/MyMediaLite" name="twitter:title" /><meta content="MyMediaLite - recommender system library for the CLR (.NET)" name="twitter:description" /><meta content="https://avatars1.githubusercontent.com/u/83638?v=3&amp;s=400" name="twitter:image:src" />
      <meta content="GitHub" property="og:site_name" /><meta content="object" property="og:type" /><meta content="https://avatars1.githubusercontent.com/u/83638?v=3&amp;s=400" property="og:image" /><meta content="zenogantner/MyMediaLite" property="og:title" /><meta content="https://github.com/zenogantner/MyMediaLite" property="og:url" /><meta content="MyMediaLite - recommender system library for the CLR (.NET)" property="og:description" />
      <meta name="browser-stats-url" content="https://api.github.com/_private/browser/stats">
    <meta name="browser-errors-url" content="https://api.github.com/_private/browser/errors">
    <link rel="assets" href="https://assets-cdn.github.com/">
    <link rel="web-socket" href="wss://live.github.com/_sockets/NjUyMzczNDowY2EzYjI0Y2E1ZDllZWVlYzA5OWI3ZWQwOGU4OWVkZDoyMDg3ZjQ4NWVlYjBjZTI1NDRlNTI4MDZkZDNjMWE4NzIzMWJhYmNjNWViYWJhMWI1MTg1N2YzNDhmNzQ0Mzhj--7cafe58652bc5488f1b68d1459a3c96947a6ac25">
    <meta name="pjax-timeout" content="1000">
    <link rel="sudo-modal" href="/sessions/sudo_modal">

    <meta name="msapplication-TileImage" content="/windows-tile.png">
    <meta name="msapplication-TileColor" content="#ffffff">
    <meta name="selected-link" value="repo_source" data-pjax-transient>

    <meta name="google-site-verification" content="KT5gs8h0wvaagLKAVWq8bbeNwnZZK1r1XQysX3xurLU">
    <meta name="google-analytics" content="UA-3769691-2">

<meta content="collector.githubapp.com" name="octolytics-host" /><meta content="github" name="octolytics-app-id" /><meta content="C3B2F806:448C:47A89FCA:568A89EE" name="octolytics-dimension-request_id" /><meta content="6523734" name="octolytics-actor-id" /><meta content="ledhead" name="octolytics-actor-login" /><meta content="b69ee87353babcdeaeeb95a7335435880dd1b5d5513578960714e14120e7300d" name="octolytics-actor-hash" />
<meta content="/&lt;user-name&gt;/&lt;repo-name&gt;/blob/show" data-pjax-transient="true" name="analytics-location" />
<meta content="Rails, view, blob#show" data-pjax-transient="true" name="analytics-event" />


  <meta class="js-ga-set" name="dimension1" content="Logged In">



        <meta name="hostname" content="github.com">
    <meta name="user-login" content="ledhead">

        <meta name="expected-hostname" content="github.com">

      <link rel="mask-icon" href="https://assets-cdn.github.com/pinned-octocat.svg" color="#4078c0">
      <link rel="icon" type="image/x-icon" href="https://assets-cdn.github.com/favicon.ico">

    <meta content="994d010cd16741de641812ad39a262e67dd44129" name="form-nonce" />

    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github-5ccfffb27ecb48e213b8582952578f145ffded2d5c7e090e9d9b98e7a9f7e4d2.css" integrity="sha256-XM//sn7LSOITuFgpUlePFF/97S1cfgkOnZuY56n35NI=" media="all" rel="stylesheet" />
    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github2-692b0b1ba861c5b4055d33b157feb7f5b35ce646135cd0db80ce5d0b2293500d.css" integrity="sha256-aSsLG6hhxbQFXTOxV/639bNc5kYTXNDbgM5dCyKTUA0=" media="all" rel="stylesheet" />
    
    


    <meta http-equiv="x-pjax-version" content="b77498504c5c93a907254bd3b087f65a">

      
  <meta name="description" content="MyMediaLite - recommender system library for the CLR (.NET)">
  <meta name="go-import" content="github.com/zenogantner/MyMediaLite git https://github.com/zenogantner/MyMediaLite.git">

  <meta content="83638" name="octolytics-dimension-user_id" /><meta content="zenogantner" name="octolytics-dimension-user_login" /><meta content="1455532" name="octolytics-dimension-repository_id" /><meta content="zenogantner/MyMediaLite" name="octolytics-dimension-repository_nwo" /><meta content="true" name="octolytics-dimension-repository_public" /><meta content="false" name="octolytics-dimension-repository_is_fork" /><meta content="1455532" name="octolytics-dimension-repository_network_root_id" /><meta content="zenogantner/MyMediaLite" name="octolytics-dimension-repository_network_root_nwo" />
  <link href="https://github.com/zenogantner/MyMediaLite/commits/master.atom" rel="alternate" title="Recent Commits to MyMediaLite:master" type="application/atom+xml">

  </head>


  <body class="logged_in   env-production windows vis-public page-blob">
    <a href="#start-of-content" tabindex="1" class="accessibility-aid js-skip-to-content">Skip to content</a>

    
    
    



      <div class="header header-logged-in true" role="banner">
  <div class="container clearfix">

    <a class="header-logo-invertocat" href="https://github.com/" data-hotkey="g d" aria-label="Homepage" data-ga-click="Header, go to dashboard, icon:logo">
  <span class="mega-octicon octicon-mark-github "></span>
</a>


      <div class="site-search repo-scope js-site-search" role="search">
          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/search" class="js-site-search-form" data-global-search-url="/search" data-repo-search-url="/zenogantner/MyMediaLite/search" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
  <label class="js-chromeless-input-container form-control">
    <div class="scope-badge">This repository</div>
    <input type="text"
      class="js-site-search-focus js-site-search-field is-clearable chromeless-input"
      data-hotkey="s"
      name="q"
      placeholder="Search"
      aria-label="Search this repository"
      data-global-scope-placeholder="Search GitHub"
      data-repo-scope-placeholder="Search"
      tabindex="1"
      autocapitalize="off">
  </label>
</form>
      </div>

      <ul class="header-nav left" role="navigation">
        <li class="header-nav-item">
          <a href="/pulls" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:pulls context:user" data-hotkey="g p" data-selected-links="/pulls /pulls/assigned /pulls/mentioned /pulls">
            Pull requests
</a>        </li>
        <li class="header-nav-item">
          <a href="/issues" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:issues context:user" data-hotkey="g i" data-selected-links="/issues /issues/assigned /issues/mentioned /issues">
            Issues
</a>        </li>
          <li class="header-nav-item">
            <a class="header-nav-link" href="https://gist.github.com/" data-ga-click="Header, go to gist, text:gist">Gist</a>
          </li>
      </ul>

    
<ul class="header-nav user-nav right" id="user-links">
  <li class="header-nav-item">
      <span class="js-socket-channel js-updatable-content"
        data-channel="notification-changed:ledhead"
        data-url="/notifications/header">
      <a href="/notifications" aria-label="You have no unread notifications" class="header-nav-link notification-indicator tooltipped tooltipped-s" data-ga-click="Header, go to notifications, icon:read" data-hotkey="g n">
          <span class="mail-status all-read"></span>
          <span class="octicon octicon-bell "></span>
</a>  </span>

  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link tooltipped tooltipped-s js-menu-target" href="/new"
       aria-label="Create new…"
       data-ga-click="Header, create new, icon:add">
      <span class="octicon octicon-plus left"></span>
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <ul class="dropdown-menu dropdown-menu-sw">
        
<a class="dropdown-item" href="/new" data-ga-click="Header, create new repository">
  New repository
</a>


  <a class="dropdown-item" href="/organizations/new" data-ga-click="Header, create new organization">
    New organization
  </a>



  <div class="dropdown-divider"></div>
  <div class="dropdown-header">
    <span title="zenogantner/MyMediaLite">This repository</span>
  </div>
    <a class="dropdown-item" href="/zenogantner/MyMediaLite/issues/new" data-ga-click="Header, create new issue">
      New issue
    </a>

      </ul>
    </div>
  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link name tooltipped tooltipped-sw js-menu-target" href="/ledhead"
       aria-label="View profile and more"
       data-ga-click="Header, show menu, icon:avatar">
      <img alt="@ledhead" class="avatar" height="20" src="https://avatars3.githubusercontent.com/u/6523734?v=3&amp;s=40" width="20" />
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <div class="dropdown-menu  dropdown-menu-sw">
        <div class=" dropdown-header header-nav-current-user css-truncate">
            Signed in as <strong class="css-truncate-target">ledhead</strong>

        </div>


        <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/ledhead" data-ga-click="Header, go to profile, text:your profile">
            Your profile
          </a>
        <a class="dropdown-item" href="/stars" data-ga-click="Header, go to starred repos, text:your stars">
          Your stars
        </a>
        <a class="dropdown-item" href="/explore" data-ga-click="Header, go to explore, text:explore">
          Explore
        </a>
          <a class="dropdown-item" href="/integrations" data-ga-click="Header, go to integrations, text:integrations">
            Integrations
          </a>
        <a class="dropdown-item" href="https://help.github.com" data-ga-click="Header, go to help, text:help">
          Help
        </a>

          <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/settings/profile" data-ga-click="Header, go to settings, icon:settings">
            Settings
          </a>

          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/logout" class="logout-form" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="vm7BJuiZgqJVa3xEnBXPGYV62s6AF54bYM+N3VRhlS4jcs7pLoisGuFpHwoAEHnKa3g24MYrz5i8gKM5rGGnkg==" /></div>
            <button class="dropdown-item dropdown-signout" data-ga-click="Header, sign out, icon:logout">
              Sign out
            </button>
</form>
      </div>
    </div>
  </li>
</ul>


    
  </div>
</div>

      

      


    <div id="start-of-content" class="accessibility-aid"></div>

      <div id="js-flash-container">
</div>


    <div role="main" class="main-content">
        <div itemscope itemtype="http://schema.org/WebPage">
    <div id="js-repo-pjax-container" class="context-loader-container js-repo-nav-next" data-pjax-container>
      
<div class="pagehead repohead instapaper_ignore readability-menu experiment-repo-nav">
  <div class="container repohead-details-container">

    

<ul class="pagehead-actions">

  <li>
        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/notifications/subscribe" class="js-social-container" data-autosubmit="true" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="EyGriX5/1slDitkBvhx299UAd8v38bQKRFYq6WmZ4HM3RxOrcCsin9RdlqglgqQ+QN3JZvDb5KNl8yGocFqsRA==" /></div>      <input id="repository_id" name="repository_id" type="hidden" value="1455532" />

        <div class="select-menu js-menu-container js-select-menu">
          <a href="/zenogantner/MyMediaLite/subscription"
            class="btn btn-sm btn-with-count select-menu-button js-menu-target" role="button" tabindex="0" aria-haspopup="true"
            data-ga-click="Repository, click Watch settings, action:blob#show">
            <span class="js-select-button">
              <span class="octicon octicon-eye "></span>
              Watch
            </span>
          </a>
          <a class="social-count js-social-count" href="/zenogantner/MyMediaLite/watchers">
            39
          </a>

        <div class="select-menu-modal-holder">
          <div class="select-menu-modal subscription-menu-modal js-menu-content" aria-hidden="true">
            <div class="select-menu-header">
              <span aria-label="Close" class="octicon octicon-x js-menu-close" role="button"></span>
              <span class="select-menu-title">Notifications</span>
            </div>

              <div class="select-menu-list js-navigation-container" role="menu">

                <div class="select-menu-item js-navigation-item selected" role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input checked="checked" id="do_included" name="do" type="radio" value="included" />
                    <span class="select-menu-item-heading">Not watching</span>
                    <span class="description">Be notified when participating or @mentioned.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-eye"></span>
                      Watch
                    </span>
                  </div>
                </div>

                <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input id="do_subscribed" name="do" type="radio" value="subscribed" />
                    <span class="select-menu-item-heading">Watching</span>
                    <span class="description">Be notified of all conversations.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-eye"></span>
                      Unwatch
                    </span>
                  </div>
                </div>

                <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input id="do_ignore" name="do" type="radio" value="ignore" />
                    <span class="select-menu-item-heading">Ignoring</span>
                    <span class="description">Never be notified.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-mute"></span>
                      Stop ignoring
                    </span>
                  </div>
                </div>

              </div>

            </div>
          </div>
        </div>
</form>
  </li>

  <li>
    
  <div class="js-toggler-container js-social-container starring-container ">

    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/unstar" class="js-toggler-form starred js-unstar-button" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="KCPVx602Ktds7Dq0eQb/2x0D9u9Z3Xnx1aPHjPhKJJR1ggCv/L6LkoP4dTWVG6aGKYoYbF9wbAnQRUtsTNz+YA==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Unstar this repository" title="Unstar zenogantner/MyMediaLite"
        data-ga-click="Repository, click unstar button, action:blob#show; text:Unstar">
        <span class="octicon octicon-star "></span>
        Unstar
      </button>
        <a class="social-count js-social-count" href="/zenogantner/MyMediaLite/stargazers">
          257
        </a>
</form>
    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/star" class="js-toggler-form unstarred js-star-button" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="+lep7pVyf6cKnrjW+dZpP565T/13NooilqH5KRX7C0kqS/EyAhXa9hd1qEDkfHXv0QqWt1NTLttzn9QD9HNzmA==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Star this repository" title="Star zenogantner/MyMediaLite"
        data-ga-click="Repository, click star button, action:blob#show; text:Star">
        <span class="octicon octicon-star "></span>
        Star
      </button>
        <a class="social-count js-social-count" href="/zenogantner/MyMediaLite/stargazers">
          257
        </a>
</form>  </div>

  </li>

  <li>
          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/fork" class="btn-with-count" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="POcooc2cgXEfwzJ5wH19XIoqoDZy3vYf8ePhBJIrcko7kTlO3SSTESR68/MY+nJSm3ZdCipE2E2eDj13nAWrLA==" /></div>
            <button
                type="submit"
                class="btn btn-sm btn-with-count"
                data-ga-click="Repository, show fork modal, action:blob#show; text:Fork"
                title="Fork your own copy of zenogantner/MyMediaLite to your account"
                aria-label="Fork your own copy of zenogantner/MyMediaLite to your account">
              <span class="octicon octicon-repo-forked "></span>
              Fork
            </button>
</form>
    <a href="/zenogantner/MyMediaLite/network" class="social-count">
      111
    </a>
  </li>
</ul>

    <h1 itemscope itemtype="http://data-vocabulary.org/Breadcrumb" class="entry-title public ">
  <span class="octicon octicon-repo "></span>
  <span class="author"><a href="/zenogantner" class="url fn" itemprop="url" rel="author"><span itemprop="title">zenogantner</span></a></span><!--
--><span class="path-divider">/</span><!--
--><strong><a href="/zenogantner/MyMediaLite" data-pjax="#js-repo-pjax-container">MyMediaLite</a></strong>

  <span class="page-context-loader">
    <img alt="" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
  </span>

</h1>

  </div>
  <div class="container">
    
<nav class="reponav js-repo-nav js-sidenav-container-pjax js-octicon-loaders"
     role="navigation"
     data-pjax="#js-repo-pjax-container">

  <a href="/zenogantner/MyMediaLite" aria-label="Code" aria-selected="true" class="js-selected-navigation-item selected reponav-item" data-hotkey="g c" data-selected-links="repo_source repo_downloads repo_commits repo_releases repo_tags repo_branches /zenogantner/MyMediaLite">
    <span class="octicon octicon-code "></span>
    Code
</a>
    <a href="/zenogantner/MyMediaLite/issues" class="js-selected-navigation-item reponav-item" data-hotkey="g i" data-selected-links="repo_issues repo_labels repo_milestones /zenogantner/MyMediaLite/issues">
      <span class="octicon octicon-issue-opened "></span>
      Issues
      <span class="counter">302</span>
</a>
  <a href="/zenogantner/MyMediaLite/pulls" class="js-selected-navigation-item reponav-item" data-hotkey="g p" data-selected-links="repo_pulls /zenogantner/MyMediaLite/pulls">
    <span class="octicon octicon-git-pull-request "></span>
    Pull requests
    <span class="counter">3</span>
</a>

  <a href="/zenogantner/MyMediaLite/pulse" class="js-selected-navigation-item reponav-item" data-selected-links="pulse /zenogantner/MyMediaLite/pulse">
    <span class="octicon octicon-pulse "></span>
    Pulse
</a>
  <a href="/zenogantner/MyMediaLite/graphs" class="js-selected-navigation-item reponav-item" data-selected-links="repo_graphs repo_contributors /zenogantner/MyMediaLite/graphs">
    <span class="octicon octicon-graph "></span>
    Graphs
</a>

</nav>

  </div>
</div>

<div class="container new-discussion-timeline experiment-repo-nav">
  <div class="repository-content">

    

<a href="/zenogantner/MyMediaLite/blob/f89e76cc31f1f7b56e20b39b156143186beabffa/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="hidden js-permalink-shortcut" data-hotkey="y">Permalink</a>

<!-- blob contrib key: blob_contributors:v21:e749c09cca31556cd078a7d7c319f534 -->

<div class="file-navigation js-zeroclipboard-container">
  
<div class="select-menu js-menu-container js-select-menu left">
  <button class="btn btn-sm select-menu-button js-menu-target css-truncate" data-hotkey="w"
    title="master"
    type="button" aria-label="Switch branches or tags" tabindex="0" aria-haspopup="true">
    <i>Branch:</i>
    <span class="js-select-button css-truncate-target">master</span>
  </button>

  <div class="select-menu-modal-holder js-menu-content js-navigation-container" data-pjax aria-hidden="true">

    <div class="select-menu-modal">
      <div class="select-menu-header">
        <span aria-label="Close" class="octicon octicon-x js-menu-close" role="button"></span>
        <span class="select-menu-title">Switch branches/tags</span>
      </div>

      <div class="select-menu-filters">
        <div class="select-menu-text-filter">
          <input type="text" aria-label="Filter branches/tags" id="context-commitish-filter-field" class="js-filterable-field js-navigation-enable" placeholder="Filter branches/tags">
        </div>
        <div class="select-menu-tabs">
          <ul>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="branches" data-filter-placeholder="Filter branches/tags" class="js-select-menu-tab" role="tab">Branches</a>
            </li>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="tags" data-filter-placeholder="Find a tag…" class="js-select-menu-tab" role="tab">Tags</a>
            </li>
          </ul>
        </div>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="branches" role="menu">

        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/zenogantner/MyMediaLite/blob/3_11_fast_knn/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="3_11_fast_knn"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="3_11_fast_knn">
                3_11_fast_knn
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open selected"
               href="/zenogantner/MyMediaLite/blob/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="master"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="master">
                master
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/zenogantner/MyMediaLite/blob/mmanzato-gsvdpp/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="mmanzato-gsvdpp"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="mmanzato-gsvdpp">
                mmanzato-gsvdpp
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/zenogantner/MyMediaLite/blob/mmanzato-korens/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="mmanzato-korens"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="mmanzato-korens">
                mmanzato-korens
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/zenogantner/MyMediaLite/blob/mml4-new-api/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="mml4-new-api"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="mml4-new-api">
                mml4-new-api
              </span>
            </a>
            <a class="select-menu-item js-navigation-item js-navigation-open "
               href="/zenogantner/MyMediaLite/blob/mml4/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
               data-name="mml4"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="mml4">
                mml4
              </span>
            </a>
        </div>

          <div class="select-menu-no-results">Nothing to show</div>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="tags">
        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.07/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.07"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.07">v3.07</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.06/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.06"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.06">v3.06</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.05/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.05"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.05">v3.05</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.04/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.04"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.04">v3.04</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.02/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.02"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.02">v3.02</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v3.0/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v3.0"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v3.0">v3.0</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v2.99/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v2.99"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v2.99">v2.99</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v2.03/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v2.03"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v2.03">v2.03</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v2.02/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v2.02"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v2.02">v2.02</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v2.01/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v2.01"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v2.01">v2.01</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v2.0/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v2.0"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v2.0">v2.0</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v1.99/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v1.99"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v1.99">v1.99</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v1.05/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v1.05"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v1.05">v1.05</a>
            </div>
            <div class="select-menu-item js-navigation-item ">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/zenogantner/MyMediaLite/tree/v1.04/src/MyMediaLite/Correlation/BinaryCorrelationType.cs"
                 data-name="v1.04"
                 data-skip-pjax="true"
                 rel="nofollow"
                 class="js-navigation-open select-menu-item-text css-truncate-target"
                 title="v1.04">v1.04</a>
            </div>
        </div>

        <div class="select-menu-no-results">Nothing to show</div>
      </div>

    </div>
  </div>
</div>

  <div class="btn-group right">
    <a href="/zenogantner/MyMediaLite/find/master"
          class="js-show-file-finder btn btn-sm"
          data-pjax
          data-hotkey="t">
      Find file
    </a>
    <button aria-label="Copy file path to clipboard" class="js-zeroclipboard btn btn-sm zeroclipboard-button tooltipped tooltipped-s" data-copied-hint="Copied!" type="button">Copy path</button>
  </div>
  <div class="breadcrumb js-zeroclipboard-target">
    <span class="repo-root js-repo-root"><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/zenogantner/MyMediaLite" class="" data-branch="master" data-pjax="true" itemscope="url"><span itemprop="title">MyMediaLite</span></a></span></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/zenogantner/MyMediaLite/tree/master/src" class="" data-branch="master" data-pjax="true" itemscope="url"><span itemprop="title">src</span></a></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/zenogantner/MyMediaLite/tree/master/src/MyMediaLite" class="" data-branch="master" data-pjax="true" itemscope="url"><span itemprop="title">MyMediaLite</span></a></span><span class="separator">/</span><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/zenogantner/MyMediaLite/tree/master/src/MyMediaLite/Correlation" class="" data-branch="master" data-pjax="true" itemscope="url"><span itemprop="title">Correlation</span></a></span><span class="separator">/</span><strong class="final-path">BinaryCorrelationType.cs</strong>
  </div>
</div>

<include-fragment class="commit-tease" src="/zenogantner/MyMediaLite/contributors/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs">
  <div>
    Fetching contributors&hellip;
  </div>

  <div class="commit-tease-contributors">
    <img alt="" class="loader-loading left" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32-EAF2F5.gif" width="16" />
    <span class="loader-error">Cannot retrieve contributors at this time</span>
  </div>
</include-fragment>
<div class="file">
  <div class="file-header">
  <div class="file-actions">

    <div class="btn-group">
      <a href="/zenogantner/MyMediaLite/raw/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="btn btn-sm " id="raw-url">Raw</a>
        <a href="/zenogantner/MyMediaLite/blame/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="btn btn-sm js-update-url-with-hash">Blame</a>
      <a href="/zenogantner/MyMediaLite/commits/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="btn btn-sm " rel="nofollow">History</a>
    </div>

        <a class="octicon-btn tooltipped tooltipped-nw"
           href="github-windows://openRepo/https://github.com/zenogantner/MyMediaLite?branch=master&amp;filepath=src%2FMyMediaLite%2FCorrelation%2FBinaryCorrelationType.cs"
           aria-label="Open this file in GitHub Desktop"
           data-ga-click="Repository, open with desktop, type:windows">
            <span class="octicon octicon-device-desktop "></span>
        </a>

        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/edit/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="inline-form js-update-url-with-hash" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="69gflB1RuTL7fVy9cXXIuvKSzA9FZMlW8SlhbvqFqqOCEfXzZrm0b7ItN5sE5iGXwexkYjnEpn/a+H83pdPiHQ==" /></div>
          <button class="octicon-btn tooltipped tooltipped-nw" type="submit"
            aria-label="Edit the file in your fork of this project" data-hotkey="e" data-disable-with>
            <span class="octicon octicon-pencil "></span>
          </button>
</form>        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/zenogantner/MyMediaLite/delete/master/src/MyMediaLite/Correlation/BinaryCorrelationType.cs" class="inline-form" data-form-nonce="994d010cd16741de641812ad39a262e67dd44129" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="h64jhpOY+L5AEIpwccSTMLOcvdDa+yO+w0bPQBbupVnLwomxGPJo7h3sEtbg6Q+niZJVffjHEtSj0VtdnCx5Bg==" /></div>
          <button class="octicon-btn octicon-btn-danger tooltipped tooltipped-nw" type="submit"
            aria-label="Delete the file in your fork of this project" data-disable-with>
            <span class="octicon octicon-trashcan "></span>
          </button>
</form>  </div>

  <div class="file-info">
      40 lines (38 sloc)
      <span class="file-info-divider"></span>
    1.37 KB
  </div>
</div>

  

  <div class="blob-wrapper data type-c">
      <table class="highlight tab-size js-file-line-container" data-tab-size="8">
      <tr>
        <td id="L1" class="blob-num js-line-number" data-line-number="1"></td>
        <td id="LC1" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// Copyright (C) 2012 Zeno Gantner</span></td>
      </tr>
      <tr>
        <td id="L2" class="blob-num js-line-number" data-line-number="2"></td>
        <td id="LC2" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// </span></td>
      </tr>
      <tr>
        <td id="L3" class="blob-num js-line-number" data-line-number="3"></td>
        <td id="LC3" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// This file is part of MyMediaLite.</span></td>
      </tr>
      <tr>
        <td id="L4" class="blob-num js-line-number" data-line-number="4"></td>
        <td id="LC4" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// </span></td>
      </tr>
      <tr>
        <td id="L5" class="blob-num js-line-number" data-line-number="5"></td>
        <td id="LC5" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// MyMediaLite is free software: you can redistribute it and/or modify</span></td>
      </tr>
      <tr>
        <td id="L6" class="blob-num js-line-number" data-line-number="6"></td>
        <td id="LC6" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// it under the terms of the GNU General Public License as published by</span></td>
      </tr>
      <tr>
        <td id="L7" class="blob-num js-line-number" data-line-number="7"></td>
        <td id="LC7" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// the Free Software Foundation, either version 3 of the License, or</span></td>
      </tr>
      <tr>
        <td id="L8" class="blob-num js-line-number" data-line-number="8"></td>
        <td id="LC8" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// (at your option) any later version.</span></td>
      </tr>
      <tr>
        <td id="L9" class="blob-num js-line-number" data-line-number="9"></td>
        <td id="LC9" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// </span></td>
      </tr>
      <tr>
        <td id="L10" class="blob-num js-line-number" data-line-number="10"></td>
        <td id="LC10" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// MyMediaLite is distributed in the hope that it will be useful,</span></td>
      </tr>
      <tr>
        <td id="L11" class="blob-num js-line-number" data-line-number="11"></td>
        <td id="LC11" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// but WITHOUT ANY WARRANTY; without even the implied warranty of</span></td>
      </tr>
      <tr>
        <td id="L12" class="blob-num js-line-number" data-line-number="12"></td>
        <td id="LC12" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the</span></td>
      </tr>
      <tr>
        <td id="L13" class="blob-num js-line-number" data-line-number="13"></td>
        <td id="LC13" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// GNU General Public License for more details.</span></td>
      </tr>
      <tr>
        <td id="L14" class="blob-num js-line-number" data-line-number="14"></td>
        <td id="LC14" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// </span></td>
      </tr>
      <tr>
        <td id="L15" class="blob-num js-line-number" data-line-number="15"></td>
        <td id="LC15" class="blob-code blob-code-inner js-file-line"><span class="pl-c">//  You should have received a copy of the GNU General Public License</span></td>
      </tr>
      <tr>
        <td id="L16" class="blob-num js-line-number" data-line-number="16"></td>
        <td id="LC16" class="blob-code blob-code-inner js-file-line"><span class="pl-c">//  along with MyMediaLite.  If not, see &lt;http://www.gnu.org/licenses/&gt;.</span></td>
      </tr>
      <tr>
        <td id="L17" class="blob-num js-line-number" data-line-number="17"></td>
        <td id="LC17" class="blob-code blob-code-inner js-file-line"><span class="pl-c">// </span></td>
      </tr>
      <tr>
        <td id="L18" class="blob-num js-line-number" data-line-number="18"></td>
        <td id="LC18" class="blob-code blob-code-inner js-file-line"><span class="pl-k">namespace</span> <span class="pl-en">MyMediaLite.Correlation</span></td>
      </tr>
      <tr>
        <td id="L19" class="blob-num js-line-number" data-line-number="19"></td>
        <td id="LC19" class="blob-code blob-code-inner js-file-line">{</td>
      </tr>
      <tr>
        <td id="L20" class="blob-num js-line-number" data-line-number="20"></td>
        <td id="LC20" class="blob-code blob-code-inner js-file-line">	<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;Correlations based on binary data&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L21" class="blob-num js-line-number" data-line-number="21"></td>
        <td id="LC21" class="blob-code blob-code-inner js-file-line">	<span class="pl-k">public</span> <span class="pl-k">enum</span> <span class="pl-en">BinaryCorrelationType</span></td>
      </tr>
      <tr>
        <td id="L22" class="blob-num js-line-number" data-line-number="22"></td>
        <td id="LC22" class="blob-code blob-code-inner js-file-line">	{</td>
      </tr>
      <tr>
        <td id="L23" class="blob-num js-line-number" data-line-number="23"></td>
        <td id="LC23" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;cosine similarity&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L24" class="blob-num js-line-number" data-line-number="24"></td>
        <td id="LC24" class="blob-code blob-code-inner js-file-line">		Cosine,</td>
      </tr>
      <tr>
        <td id="L25" class="blob-num js-line-number" data-line-number="25"></td>
        <td id="LC25" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;Jaccard index (Tanimoto coefficient)&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L26" class="blob-num js-line-number" data-line-number="26"></td>
        <td id="LC26" class="blob-code blob-code-inner js-file-line">		Jaccard,</td>
      </tr>
      <tr>
        <td id="L27" class="blob-num js-line-number" data-line-number="27"></td>
        <td id="LC27" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;conditional probability&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L28" class="blob-num js-line-number" data-line-number="28"></td>
        <td id="LC28" class="blob-code blob-code-inner js-file-line">		ConditionalProbability,</td>
      </tr>
      <tr>
        <td id="L29" class="blob-num js-line-number" data-line-number="29"></td>
        <td id="LC29" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;bidirectional conditional probability&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L30" class="blob-num js-line-number" data-line-number="30"></td>
        <td id="LC30" class="blob-code blob-code-inner js-file-line">		BidirectionalConditionalProbability,</td>
      </tr>
      <tr>
        <td id="L31" class="blob-num js-line-number" data-line-number="31"></td>
        <td id="LC31" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;cooccurrence counts&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L32" class="blob-num js-line-number" data-line-number="32"></td>
        <td id="LC32" class="blob-code blob-code-inner js-file-line">		Cooccurrence,</td>
      </tr>
      <tr>
        <td id="L33" class="blob-num js-line-number" data-line-number="33"></td>
        <td id="LC33" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;use a similarity provider to get the correlation&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L34" class="blob-num js-line-number" data-line-number="34"></td>
        <td id="LC34" class="blob-code blob-code-inner js-file-line">		SimilarityProvider,</td>
      </tr>
      <tr>
        <td id="L35" class="blob-num js-line-number" data-line-number="35"></td>
        <td id="LC35" class="blob-code blob-code-inner js-file-line">		<span class="pl-c">/// &lt;<span class="pl-ent">summary</span>&gt;use stored/precomputed correlation&lt;/<span class="pl-ent">summary</span>&gt;</span></td>
      </tr>
      <tr>
        <td id="L36" class="blob-num js-line-number" data-line-number="36"></td>
        <td id="LC36" class="blob-code blob-code-inner js-file-line">		Stored</td>
      </tr>
      <tr>
        <td id="L37" class="blob-num js-line-number" data-line-number="37"></td>
        <td id="LC37" class="blob-code blob-code-inner js-file-line">	}</td>
      </tr>
      <tr>
        <td id="L38" class="blob-num js-line-number" data-line-number="38"></td>
        <td id="LC38" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L39" class="blob-num js-line-number" data-line-number="39"></td>
        <td id="LC39" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
</table>

  </div>

</div>

<a href="#jump-to-line" rel="facebox[.linejump]" data-hotkey="l" style="display:none">Jump to Line</a>
<div id="jump-to-line" style="display:none">
  <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="" class="js-jump-to-line-form" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
    <input class="linejump-input js-jump-to-line-field" type="text" placeholder="Jump to line&hellip;" aria-label="Jump to line" autofocus>
    <button type="submit" class="btn">Go</button>
</form></div>

  </div>
  <div class="modal-backdrop"></div>
</div>

    </div>
  </div>

    </div>

        <div class="container">
  <div class="site-footer" role="contentinfo">
    <ul class="site-footer-links right">
        <li><a href="https://status.github.com/" data-ga-click="Footer, go to status, text:status">Status</a></li>
      <li><a href="https://developer.github.com" data-ga-click="Footer, go to api, text:api">API</a></li>
      <li><a href="https://training.github.com" data-ga-click="Footer, go to training, text:training">Training</a></li>
      <li><a href="https://shop.github.com" data-ga-click="Footer, go to shop, text:shop">Shop</a></li>
        <li><a href="https://github.com/blog" data-ga-click="Footer, go to blog, text:blog">Blog</a></li>
        <li><a href="https://github.com/about" data-ga-click="Footer, go to about, text:about">About</a></li>
        <li><a href="https://github.com/pricing" data-ga-click="Footer, go to pricing, text:pricing">Pricing</a></li>

    </ul>

    <a href="https://github.com" aria-label="Homepage">
      <span class="mega-octicon octicon-mark-github " title="GitHub "></span>
</a>
    <ul class="site-footer-links">
      <li>&copy; 2016 <span title="0.07492s from github-fe119-cp1-prd.iad.github.net">GitHub</span>, Inc.</li>
        <li><a href="https://github.com/site/terms" data-ga-click="Footer, go to terms, text:terms">Terms</a></li>
        <li><a href="https://github.com/site/privacy" data-ga-click="Footer, go to privacy, text:privacy">Privacy</a></li>
        <li><a href="https://github.com/security" data-ga-click="Footer, go to security, text:security">Security</a></li>
        <li><a href="https://github.com/contact" data-ga-click="Footer, go to contact, text:contact">Contact</a></li>
        <li><a href="https://help.github.com" data-ga-click="Footer, go to help, text:help">Help</a></li>
    </ul>
  </div>
</div>



    
    
    

    <div id="ajax-error-message" class="flash flash-error">
      <span class="octicon octicon-alert"></span>
      <button type="button" class="flash-close js-flash-close js-ajax-error-dismiss" aria-label="Dismiss error">
        <span class="octicon octicon-x"></span>
      </button>
      Something went wrong with that request. Please try again.
    </div>


      <script crossorigin="anonymous" integrity="sha256-7460qJ7p88i3YTMH/liaj1cFgX987ie+xRzl6WMjSr8=" src="https://assets-cdn.github.com/assets/frameworks-ef8eb4a89ee9f3c8b7613307fe589a8f5705817f7cee27bec51ce5e963234abf.js"></script>
      <script async="async" crossorigin="anonymous" integrity="sha256-S2uOfRHrt7zoUSbTtBMMgAQfKubV1u+JAajAw/fLgNI=" src="https://assets-cdn.github.com/assets/github-4b6b8e7d11ebb7bce85126d3b4130c80041f2ae6d5d6ef8901a8c0c3f7cb80d2.js"></script>
      
      
      
    <div class="js-stale-session-flash stale-session-flash flash flash-warn flash-banner hidden">
      <span class="octicon octicon-alert"></span>
      <span class="signed-in-tab-flash">You signed in with another tab or window. <a href="">Reload</a> to refresh your session.</span>
      <span class="signed-out-tab-flash">You signed out in another tab or window. <a href="">Reload</a> to refresh your session.</span>
    </div>
  </body>
</html>

