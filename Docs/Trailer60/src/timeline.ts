import {beatF} from './theme';

// Editing source for copy, shot order and timing. Durations are measured in beats.
const content = [
  {id:'intro', title:'Accounts.\nSubscriptions.\nBuilt for Unity.', copy:'Unity UI + PHP backend\n+ Stripe subscriptions.', caption:'Start with accounts and subscriptions built.\nConfigure your backend and Stripe.', visual:'entry.png', motion:'single-screen-arrival'},
  {id:'design', title:'Playful menus.\nClear choices.', copy:'Malay interface.\nCentered actions.\nTeal and gold. Storybook art.', caption:'Edit the Malay menus in Unity scenes\nand the shared button prefab.', visual:'entry.png', motion:'camera-detail'},
  {id:'register', title:'Create an account.', copy:'Name, email, phone and password.\nValidation stays close to the form.', caption:'Choose Daftar akaun to register.\nKembali takes you back.', visual:'SignUp.png', motion:'cursor-panel-open'},
  {id:'login', title:'Sign in.\nFind your player space.', copy:'Profile and subscription actions,\ntogether in one menu.', caption:'Log masuk opens sign-in.\nFind account actions in the player menu.', visual:'SignIn.png', motion:'login-to-home'},
  {id:'profile', title:'Know your player.', copy:'Profile details. Email verification.\nRefresh from the same panel.', caption:'View your profile and send a\nverification email from the same panel.', visual:'Profile.png', motion:'profile-open'},
  {id:'subscribe', title:'Subscriptions,\nin the same flow.', copy:'Review subscription details.\nOpen billing from the player menu.', caption:'Langganan saya shows your plan.\nLanggan opens the billing handoff.', visual:'SubscriptionDetails.png', motion:'subscription-open'},
  {id:'payment', title:'The payment starts\nwith your server.', copy:'Unity requests a ticket.\nPHP opens the Stripe checkout route.', caption:'Illustrated: a ticket opens checkout.\nNo live Stripe transaction is shown.', visual:'diagram', motion:'ticket-to-checkout'},
  {id:'confirmation', title:'Access follows\nconfirmed state.', copy:'Stripe sends a signed webhook.\nThe server updates. Unity refreshes.', caption:'Browser return never proves payment.\nThe server confirms subscription access.', visual:'diagram', motion:'webhook-return'},
  {id:'access', title:'Keep access honest.', copy:'Fresh server state and\npaid-through time govern access.', caption:'The UI shows current manager state.\nLocal preferences never prove payment.', visual:'SubscriptionDetails.png', motion:'state-detail'},
  {id:'extras', title:'Small details.\nBetter control.', copy:'Motion preference\nKeyboard focus\nDevice-conflict recovery', caption:'Navigate with keys, reduce motion,\nand recover access on another device.', visual:'filmstrip', motion:'word-relay-filmstrip'},
  {id:'customize', title:'Make it yours.', copy:'Edit scenes, prefabs and theme.\nUse the included PDF tutorials.', caption:'Change colors, labels and timing\nwith ordinary Unity editing tools.', visual:'tutorials', motion:'source-and-guides'},
  {id:'outro', title:'Unity subscription\ntemplate', copy:'', caption:'Customize menus. Connect PHP and Stripe.\nBuild your game.', visual:'group', motion:'product-group-settle'},
];

export const SHOTS = content.map((shot,index) => ({
  ...shot, from:beatF(index*10), duration:beatF(10),
  narrativeGoal:shot.title.replace('\n',' '), assetRefs:[shot.visual],
}));
export type Shot = typeof SHOTS[number];

// All audio cues use the same frame grid as the picture. No timing table in a second file.
export const SFX = [
  {from:15,kind:'settle'}, {from:165,kind:'sweep'},
  {from:345,kind:'click'}, {from:495,kind:'click'},
  {from:570,kind:'sweep'}, {from:645,kind:'click'},
  {from:795,kind:'click'}, {from:930,kind:'tick'},
  {from:975,kind:'tick'}, {from:1020,kind:'tick'},
  {from:1080,kind:'tick'}, {from:1125,kind:'tick'}, {from:1170,kind:'tick'},
  {from:1230,kind:'sweep'}, {from:1358,kind:'step'},
  {from:1402,kind:'step'}, {from:1446,kind:'step'},
  {from:1530,kind:'paper'}, {from:1680,kind:'settle'},
] as const;
