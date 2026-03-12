// ─────────────────────────────────────────────────────────────────
// DinoSaw Survival – Cloudflare Worker (dashboard paste version)
// Paste this entire file in:
//   dash.cloudflare.com → Workers & Pages → your worker → Edit Code
// ─────────────────────────────────────────────────────────────────

const HTML = `<!DOCTYPE html>
<html lang="ro">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>DinoSaw Survival</title>
  <meta name="description" content="Joc survival: dinozauri vs fierăstraie rotative." />
  <style>
    *,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
    body{background:#0a0a1a;display:flex;flex-direction:column;align-items:center;justify-content:center;min-height:100vh;font-family:'Segoe UI',system-ui,sans-serif;overflow:hidden}
    #ui-bar{display:flex;gap:40px;padding:10px 30px;background:rgba(255,255,255,.05);border:1px solid rgba(255,255,255,.1);border-radius:12px;margin-bottom:12px;backdrop-filter:blur(10px)}
    .stat{text-align:center}
    .stat-label{font-size:10px;letter-spacing:2px;color:#888;text-transform:uppercase}
    .stat-value{font-size:24px;font-weight:700;color:#fff}
    #wave-value{color:#f59e0b}#score-value{color:#34d399}#kills-value{color:#f87171}
    #canvas{border-radius:12px;border:2px solid rgba(255,255,255,.08);box-shadow:0 0 60px rgba(99,102,241,.3);display:block}
    #overlay{position:absolute;inset:0;display:flex;flex-direction:column;align-items:center;justify-content:center;background:rgba(0,0,0,.75);backdrop-filter:blur(6px);gap:16px;border-radius:12px;pointer-events:none}
    #overlay h1{font-size:52px;font-weight:900;color:#fff;letter-spacing:-1px;text-shadow:0 0 40px rgba(245,158,11,.8)}
    #overlay p{color:#ccc;font-size:16px}
    #start-btn{margin-top:8px;padding:14px 48px;font-size:18px;font-weight:700;background:linear-gradient(135deg,#f59e0b,#ef4444);color:#fff;border:none;border-radius:999px;cursor:pointer;pointer-events:auto;transition:transform .15s,box-shadow .15s;box-shadow:0 4px 20px rgba(239,68,68,.4)}
    #start-btn:hover{transform:scale(1.05);box-shadow:0 6px 30px rgba(239,68,68,.6)}
    #start-btn:active{transform:scale(.97)}
    #canvas-wrapper{position:relative}
  </style>
</head>
<body>
<div id="ui-bar">
  <div class="stat"><div class="stat-label">Val</div><div class="stat-value" id="wave-value">1</div></div>
  <div class="stat"><div class="stat-label">Scor</div><div class="stat-value" id="score-value">0</div></div>
  <div class="stat"><div class="stat-label">Uci&#x219;i</div><div class="stat-value" id="kills-value">0</div></div>
</div>
<div id="canvas-wrapper">
  <canvas id="canvas" width="960" height="480"></canvas>
  <div id="overlay">
    <h1>&#x1F995; DinoSaw</h1>
    <p>Dinozaurii merg spre moarte. Tu doar prive&#x219;ti.</p>
    <button id="start-btn">START JOC</button>
  </div>
</div>
<script>
const CFG={canvas:{w:960,h:480},spawnIntervalStart:1400,spawnIntervalMin:200,spawnRateMultiplier:1.25,waveDuration:15000,dinoW:54,dinoH:38,dinoSpeedMin:60,dinoSpeedMax:120,dinoHPStart:40,dinoHPBonus:15,dinoColors:['#4ade80','#86efac','#6ee7b7','#34d399','#a3e635'],sawRadius:22,sawDPS:30,sawRotSpeed:6,sawCols:2,sawRows:9,sawSpacingY:50,sawSpacingX:48,sawWallX:820,sawWallYStart:30,pointsPerKill:10,particleCount:14};
const canvas=document.getElementById('canvas'),ctx=canvas.getContext('2d'),overlay=document.getElementById('overlay');
let state='idle',score=0,kills=0,wave=1,spawnInterval=CFG.spawnIntervalStart,spawnTimer=0,waveTimer=0,lastTime=0,dinosaurs=[],saws=[],particles=[];

class Dinosaur{constructor(w){this.x=-CFG.dinoW;this.y=CFG.canvas.h-80;this.w=CFG.dinoW;this.h=CFG.dinoH;this.speed=CFG.dinoSpeedMin+Math.random()*(CFG.dinoSpeedMax-CFG.dinoSpeedMin);this.maxHP=CFG.dinoHPStart+w*CFG.dinoHPBonus;this.hp=this.maxHP;this.color=CFG.dinoColors[Math.floor(Math.random()*CFG.dinoColors.length)];this.scale=0.8+Math.random()*0.4;this.dead=false;this.bobPhase=Math.random()*Math.PI*2}
update(dt){this.x+=this.speed*dt;this.bobPhase+=8*dt}
takeDamage(d){if(this.dead)return;this.hp-=d;if(this.hp<=0)this.die()}
die(){if(this.dead)return;this.dead=true;kills++;score+=CFG.pointsPerKill;updateUI();for(let i=0;i<CFG.particleCount;i++)particles.push(new Particle(this.x+this.w/2,this.y+this.h/2))}
draw(ctx){const bobY=Math.sin(this.bobPhase)*2,cx=this.x+this.w/2,cy=this.y+this.h/2+bobY;ctx.save();ctx.translate(cx,cy);ctx.scale(this.scale,this.scale);ctx.fillStyle=this.color;ctx.beginPath();ctx.ellipse(0,0,this.w/2,this.h/2,0,0,Math.PI*2);ctx.fill();ctx.beginPath();ctx.ellipse(this.w*.35,-this.h*.25,this.h*.28,this.h*.24,0,0,Math.PI*2);ctx.fill();ctx.fillStyle='#1a1a2e';ctx.beginPath();ctx.arc(this.w*.42,-this.h*.3,3,0,Math.PI*2);ctx.fill();ctx.strokeStyle=this.color;ctx.lineWidth=5;ctx.lineCap='round';ctx.beginPath();ctx.moveTo(-this.w*.5,0);ctx.quadraticCurveTo(-this.w*.75,this.h*.2,-this.w*.9,-this.h*.1);ctx.stroke();ctx.lineWidth=4;[[-0.15,0.45],[0.1,0.45]].forEach(([lx,ly])=>{ctx.beginPath();ctx.moveTo(lx*this.w,ly*this.h);ctx.lineTo((lx-0.04)*this.w,(ly+0.35)*this.h);ctx.stroke()});ctx.restore();const bW=this.w*this.scale,bH=4,bx=this.x+(this.w-bW)/2,by=this.y-10+bobY;ctx.fillStyle='rgba(0,0,0,.5)';ctx.fillRect(bx,by,bW,bH);ctx.fillStyle=this.hp/this.maxHP>0.5?'#4ade80':this.hp/this.maxHP>0.25?'#fbbf24':'#f87171';ctx.fillRect(bx,by,bW*(this.hp/this.maxHP),bH)}}

class Saw{constructor(x,y){this.x=x;this.y=y;this.r=CFG.sawRadius;this.rot=Math.random()*Math.PI*2}
update(dt){this.rot+=CFG.sawRotSpeed*dt}
overlaps(d){const cx=d.x+d.w/2,cy=d.y+d.h/2,hw=(d.w*d.scale)/2,hh=(d.h*d.scale)/2,nX=Math.max(cx-hw,Math.min(cx,cx+hw)),nY=Math.max(cy-hh,Math.min(cy,cy+hh)),dx=this.x-nX,dy=this.y-nY;return dx*dx+dy*dy<this.r*this.r}
draw(ctx){ctx.save();ctx.translate(this.x,this.y);const g=ctx.createRadialGradient(0,0,this.r*.3,0,0,this.r*1.5);g.addColorStop(0,'rgba(239,68,68,.3)');g.addColorStop(1,'rgba(239,68,68,0)');ctx.fillStyle=g;ctx.beginPath();ctx.arc(0,0,this.r*1.5,0,Math.PI*2);ctx.fill();ctx.rotate(this.rot);const t=10;ctx.fillStyle='#94a3b8';ctx.beginPath();for(let i=0;i<t;i++){const a=(i/t)*Math.PI*2,a2=((i+.5)/t)*Math.PI*2,ir=this.r*.65,tip=this.r*1.22;ctx.lineTo(Math.cos(a)*ir,Math.sin(a)*ir);ctx.lineTo(Math.cos(a2-.08)*tip,Math.sin(a2-.08)*tip);ctx.lineTo(Math.cos(a2+.08)*tip,Math.sin(a2+.08)*tip);ctx.lineTo(Math.cos(a+(1/t)*Math.PI*2)*ir,Math.sin(a+(1/t)*Math.PI*2)*ir)}ctx.closePath();ctx.fill();ctx.fillStyle='#e2e8f0';ctx.beginPath();ctx.arc(0,0,this.r*.55,0,Math.PI*2);ctx.fill();ctx.fillStyle='#475569';ctx.beginPath();ctx.arc(0,0,this.r*.15,0,Math.PI*2);ctx.fill();ctx.restore()}}

class Particle{constructor(x,y){this.x=x;this.y=y;const a=Math.random()*Math.PI*2,s=60+Math.random()*180;this.vx=Math.cos(a)*s;this.vy=Math.sin(a)*s-60;this.life=1;this.decay=0.9+Math.random()*.8;this.size=4+Math.random()*6;this.color='hsl('+(Math.random()*30)+',90%,55%)';this.dead=false}
update(dt){this.x+=this.vx*dt;this.y+=this.vy*dt;this.vy+=250*dt;this.life-=this.decay*dt;if(this.life<=0)this.dead=true}
draw(ctx){ctx.globalAlpha=Math.max(0,this.life);ctx.fillStyle=this.color;ctx.beginPath();ctx.arc(this.x,this.y,this.size*this.life,0,Math.PI*2);ctx.fill();ctx.globalAlpha=1}}

function initSaws(){saws=[];for(let c=0;c<CFG.sawCols;c++)for(let r=0;r<CFG.sawRows;r++)saws.push(new Saw(CFG.sawWallX+c*CFG.sawSpacingX,CFG.sawWallYStart+r*CFG.sawSpacingY))}
function spawnDino(){const d=new Dinosaur(wave);d.y=CFG.canvas.h-80-Math.random()*20;dinosaurs.push(d)}

const STARS=Array.from({length:80},()=>({x:Math.random()*960,y:Math.random()*300,r:Math.random()*1.2+.3,a:Math.random()}));
function drawStars(){STARS.forEach(s=>{ctx.globalAlpha=s.a*(.4+.3*Math.sin(Date.now()/800+s.x));ctx.fillStyle='#e2e8f0';ctx.beginPath();ctx.arc(s.x,s.y,s.r,0,Math.PI*2);ctx.fill()});ctx.globalAlpha=1}

function draw(){const W=CFG.canvas.w,H=CFG.canvas.h,bg=ctx.createLinearGradient(0,0,0,H);bg.addColorStop(0,'#0f172a');bg.addColorStop(1,'#1e293b');ctx.fillStyle=bg;ctx.fillRect(0,0,W,H);drawStars();const gY=H-55,fg=ctx.createLinearGradient(0,gY,0,H);fg.addColorStop(0,'#334155');fg.addColorStop(1,'#1e293b');ctx.fillStyle=fg;ctx.fillRect(0,gY,W,H-gY);ctx.strokeStyle='rgba(148,163,184,.3)';ctx.lineWidth=2;ctx.beginPath();ctx.moveTo(0,gY);ctx.lineTo(W,gY);ctx.stroke();ctx.strokeStyle='rgba(148,163,184,.06)';ctx.lineWidth=1;for(let i=0;i<8;i++){ctx.beginPath();ctx.moveTo(0,gY+6+i*6);ctx.lineTo(W,gY+6+i*6);ctx.stroke()}const wX=CFG.sawWallX-CFG.sawRadius-10,wW=CFG.sawCols*CFG.sawSpacingX+CFG.sawRadius*2+20;ctx.fillStyle='rgba(15,23,42,.7)';ctx.fillRect(wX,0,wW,H);ctx.fillStyle='rgba(239,68,68,.08)';ctx.fillRect(wX,0,wW,H);ctx.strokeStyle='rgba(239,68,68,.4)';ctx.lineWidth=2;ctx.setLineDash([8,6]);ctx.beginPath();ctx.moveTo(wX,0);ctx.lineTo(wX,H);ctx.stroke();ctx.setLineDash([]);dinosaurs.forEach(d=>d.draw(ctx));saws.forEach(s=>s.draw(ctx));particles.forEach(p=>p.draw(ctx));const pulse=.5+.5*Math.sin(Date.now()/300);ctx.fillStyle='rgba(99,102,241,'+(0.15+pulse*.15)+')';ctx.fillRect(0,0,12,H)}

function updateUI(){document.getElementById('wave-value').textContent=wave;document.getElementById('score-value').textContent=score;document.getElementById('kills-value').textContent=kills}

function gameLoop(ts){if(state!=='running')return;requestAnimationFrame(gameLoop);const dt=Math.min((ts-lastTime)/1000,.05);lastTime=ts;spawnTimer+=dt*1000;if(spawnTimer>=spawnInterval){spawnTimer=0;spawnDino()}waveTimer+=dt*1000;if(waveTimer>=CFG.waveDuration){waveTimer=0;wave++;spawnInterval=Math.max(CFG.spawnIntervalMin,spawnInterval/CFG.spawnRateMultiplier);updateUI()}dinosaurs.forEach(d=>d.update(dt));saws.forEach(s=>s.update(dt));particles.forEach(p=>p.update(dt));saws.forEach(saw=>dinosaurs.forEach(dino=>{if(!dino.dead&&saw.overlaps(dino))dino.takeDamage(CFG.sawDPS*dt)}));dinosaurs=dinosaurs.filter(d=>!d.dead&&d.x<CFG.canvas.w+100);particles=particles.filter(p=>!p.dead);draw()}

function startGame(){score=0;kills=0;wave=1;spawnInterval=CFG.spawnIntervalStart;spawnTimer=0;waveTimer=0;dinosaurs=[];particles=[];updateUI();initSaws();overlay.style.display='none';state='running';lastTime=performance.now();requestAnimationFrame(gameLoop)}

document.getElementById('start-btn').addEventListener('click',startGame);
initSaws();draw();
<\/script>
</body>
</html>`;

export default {
    async fetch(request) {
        const url = new URL(request.url);

        // Serve only root path
        if (url.pathname === '/' || url.pathname === '/index.html') {
            return new Response(HTML, {
                headers: {
                    'Content-Type': 'text/html;charset=UTF-8',
                    'Cache-Control': 'public, max-age=3600',
                },
            });
        }

        // 404 for everything else
        return new Response('Not found', { status: 404 });
    },
};
